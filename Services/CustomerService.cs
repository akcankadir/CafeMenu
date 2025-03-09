using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CafeMenu.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CafeMenu.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly string _connectionString;
        private readonly ICacheService _cacheService;
        private readonly IExchangeRateService _exchangeRateService;

        public CustomerService(
            IConfiguration configuration,
            ICacheService cacheService,
            IExchangeRateService exchangeRateService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _cacheService = cacheService;
            _exchangeRateService = exchangeRateService;
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync(int tenantId)
        {
            var cacheKey = $"customer_categories_tenant_{tenantId}";

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"
                    WITH CategoryCTE AS (
                        SELECT 
                            c.CategoryId,
                            c.CategoryName,
                            c.ParentCategoryId,
                            COUNT(p.ProductId) as ProductCount
                        FROM Categories c
                        LEFT JOIN Products p ON c.CategoryId = p.CategoryId 
                            AND p.IsDeleted = 0
                        WHERE c.TenantId = @TenantId 
                            AND c.IsDeleted = 0
                        GROUP BY c.CategoryId, c.CategoryName, c.ParentCategoryId
                    )
                    SELECT * FROM CategoryCTE
                    ORDER BY ParentCategoryId, CategoryName";

                var categories = await connection.QueryAsync<CategoryViewModel>(sql, new { TenantId = tenantId });
                var categoryList = categories.ToList();

                var rootCategories = categoryList
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();

                foreach (var rootCategory in rootCategories)
                {
                    BuildCategoryHierarchy(rootCategory, categoryList);
                }

                return rootCategories;
            });
        }

        private void BuildCategoryHierarchy(CategoryViewModel parent, List<CategoryViewModel> allCategories)
        {
            parent.SubCategories = allCategories
                .Where(c => c.ParentCategoryId == parent.CategoryId)
                .ToList();

            foreach (var child in parent.SubCategories)
            {
                BuildCategoryHierarchy(child, allCategories);
            }
        }

        public async Task<(List<ProductViewModel> Products, int TotalCount)> GetProductsAsync(
            int tenantId,
            int? categoryId,
            int page,
            int pageSize,
            string currency = "TRY")
        {
            var cacheKey = $"customer_products_tenant_{tenantId}_category_{categoryId}_page_{page}";

            var result = await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var connection = new SqlConnection(_connectionString);
                var offset = (page - 1) * pageSize;

                var sql = @"
                    SELECT 
                        p.ProductId, p.ProductName, p.CategoryId, 
                        p.Price, p.ImagePath, c.CategoryName,
                        COUNT(*) OVER() as TotalCount
                    FROM Products p
                    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                    WHERE p.TenantId = @TenantId 
                        AND p.IsDeleted = 0
                        AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                    ORDER BY p.ProductName
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                var products = await connection.QueryAsync<ProductViewModel>(
                    sql,
                    new { TenantId = tenantId, CategoryId = categoryId, Offset = offset, PageSize = pageSize }
                );

                var productList = products.ToList();
                var totalCount = productList.Any() ? productList[0].TotalCount : 0;

                // Ürün özelliklerini getir
                foreach (var product in productList)
                {
                    product.Properties = await GetProductPropertiesAsync(connection, product.ProductId);
                }

                return (Products: productList, TotalCount: totalCount);
            });

            if (currency != "TRY")
            {
                var rate = await _exchangeRateService.GetRateAsync(currency);
                if (rate != null)
                {
                    foreach (var product in result.Products)
                    {
                        product.Price = await _exchangeRateService.ConvertCurrencyAsync(
                            product.Price, "TRY", currency);
                    }
                }
            }

            return result;
        }

        private async Task<List<PropertyViewModel>> GetProductPropertiesAsync(SqlConnection connection, int productId)
        {
            var sql = @"
                SELECT 
                    p.PropertyId,
                    p.[Key],
                    p.Value
                FROM Properties p
                INNER JOIN ProductProperties pp ON p.PropertyId = pp.PropertyId
                WHERE pp.ProductId = @ProductId
                    AND pp.IsDeleted = 0";

            var properties = await connection.QueryAsync<PropertyViewModel>(sql, new { ProductId = productId });
            return properties.ToList();
        }

        public async Task<CustomerHomeViewModel> GetHomePageDataAsync(int tenantId, string currency = "TRY")
        {
            var categories = await GetCategoriesAsync(tenantId);
            var exchangeRates = await GetExchangeRatesAsync();
            
            // Öne çıkan ürünleri al
            var featuredProducts = await GetFeaturedProductsAsync(tenantId, currency);
            
            // Yeni ürünleri al
            var newProducts = await GetNewProductsAsync(tenantId, currency);
            
            return new CustomerHomeViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts,
                NewProducts = newProducts,
                ExchangeRates = exchangeRates,
                SelectedCurrency = currency
            };
        }

        public async Task<CategoryViewModel> GetCategoryProductsAsync(int categoryId, int tenantId, string currency = "TRY", int page = 1, int pageSize = 12)
        {
            var cacheKey = $"category_products_tenant_{tenantId}_category_{categoryId}_page_{page}_size_{pageSize}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var connection = new SqlConnection(_connectionString);
                
                // Kategori bilgilerini al
                var categorySql = @"
                    SELECT 
                        c.CategoryId,
                        c.CategoryName,
                        c.ParentCategoryId,
                        c.ImagePath
                    FROM Categories c
                    WHERE c.CategoryId = @CategoryId 
                        AND c.TenantId = @TenantId 
                        AND c.IsDeleted = 0";
                        
                var category = await connection.QueryFirstOrDefaultAsync<CategoryViewModel>(
                    categorySql,
                    new { CategoryId = categoryId, TenantId = tenantId }
                );
                
                if (category == null)
                    return null;
                    
                // Alt kategorileri al
                var subCategoriesSql = @"
                    SELECT 
                        c.CategoryId,
                        c.CategoryName,
                        c.ParentCategoryId,
                        c.ImagePath
                    FROM Categories c
                    WHERE c.ParentCategoryId = @CategoryId 
                        AND c.TenantId = @TenantId 
                        AND c.IsDeleted = 0";
                        
                var subCategories = await connection.QueryAsync<CategoryViewModel>(
                    subCategoriesSql,
                    new { CategoryId = categoryId, TenantId = tenantId }
                );
                
                category.SubCategories = subCategories.ToList();
                
                // Kategori ürünlerini al
                var offset = (page - 1) * pageSize;
                var productsSql = @"
                    SELECT 
                        p.ProductId, 
                        p.ProductName, 
                        p.CategoryId, 
                        p.Price, 
                        p.ImagePath, 
                        c.CategoryName,
                        COUNT(*) OVER() as TotalCount
                    FROM Products p
                    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                    WHERE p.CategoryId = @CategoryId 
                        AND p.TenantId = @TenantId 
                        AND p.IsDeleted = 0
                    ORDER BY p.ProductName
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";
                    
                var products = await connection.QueryAsync<ProductListItemViewModel>(
                    productsSql,
                    new { CategoryId = categoryId, TenantId = tenantId, Offset = offset, PageSize = pageSize }
                );
                
                var productList = products.ToList();
                var totalCount = productList.Any() ? productList.First().TotalCount : 0;
                
                // Döviz kuru dönüşümü
                if (currency != "TRY")
                {
                    var rate = await _exchangeRateService.GetRateAsync(currency);
                    if (rate != null)
                    {
                        foreach (var product in productList)
                        {
                            var convertedPrice = await _exchangeRateService.ConvertCurrencyAsync(
                                product.Price, "TRY", currency);
                            product.Price = convertedPrice;
                            product.FormattedPrice = $"{convertedPrice:N2} {currency}";
                        }
                    }
                }
                else
                {
                    foreach (var product in productList)
                    {
                        product.FormattedPrice = $"{product.Price:N2} TRY";
                    }
                }
                
                category.Products = productList;
                category.PaginationInfo = new PaginationInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalCount
                };
                
                category.ExchangeRates = await GetExchangeRatesAsync();
                category.SelectedCurrency = currency;
                
                return category;
            });
        }

        public async Task<ProductDetailViewModel> GetProductDetailsAsync(int productId, int tenantId, string currency = "TRY")
        {
            var cacheKey = $"product_detail_tenant_{tenantId}_product_{productId}";
            
            var product = await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var connection = new SqlConnection(_connectionString);
                
                // Ürün bilgilerini al
                var productSql = @"
                    SELECT 
                        p.ProductId, 
                        p.ProductName, 
                        p.CategoryId,
                        p.Price, 
                        p.ImagePath, 
                        c.CategoryName
                    FROM Products p
                    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                    WHERE p.ProductId = @ProductId 
                        AND p.TenantId = @TenantId 
                        AND p.IsDeleted = 0";
                        
                var product = await connection.QueryFirstOrDefaultAsync<ProductDetailViewModel>(
                    productSql,
                    new { ProductId = productId, TenantId = tenantId }
                );
                
                if (product == null)
                    return null;
                    
                // Ürün özelliklerini al
                var propertiesSql = @"
                    SELECT 
                        p.PropertyId,
                        p.[Key],
                        p.Value
                    FROM Properties p
                    INNER JOIN ProductProperties pp ON p.PropertyId = pp.PropertyId
                    WHERE pp.ProductId = @ProductId
                        AND pp.TenantId = @TenantId";
                        
                var properties = await connection.QueryAsync<PropertyViewModel>(
                    propertiesSql,
                    new { ProductId = productId, TenantId = tenantId }
                );
                
                product.Properties = properties.ToList();
                
                // İlgili ürünleri al (aynı kategorideki diğer ürünler)
                var relatedSql = @"
                    SELECT TOP 4
                        p.ProductId, 
                        p.ProductName, 
                        p.CategoryId,
                        p.Price, 
                        p.ImagePath, 
                        c.CategoryName
                    FROM Products p
                    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                    WHERE p.CategoryId = @CategoryId 
                        AND p.ProductId != @ProductId
                        AND p.TenantId = @TenantId 
                        AND p.IsDeleted = 0
                    ORDER BY NEWID()";
                        
                var relatedProducts = await connection.QueryAsync<ProductListItemViewModel>(
                    relatedSql,
                    new { CategoryId = product.CategoryId, ProductId = productId, TenantId = tenantId }
                );
                
                product.RelatedProducts = relatedProducts.ToList();
                
                return product;
            });
            
            if (product == null)
                return null;
                
            // Döviz kuru dönüşümü
            if (currency != "TRY")
            {
                var rate = await _exchangeRateService.GetRateAsync(currency);
                if (rate != null)
                {
                    product.Price = await _exchangeRateService.ConvertCurrencyAsync(
                        product.Price, "TRY", currency);
                    product.FormattedPrice = $"{product.Price:N2} {currency}";
                    
                    foreach (var relatedProduct in product.RelatedProducts)
                    {
                        relatedProduct.Price = await _exchangeRateService.ConvertCurrencyAsync(
                            relatedProduct.Price, "TRY", currency);
                        relatedProduct.FormattedPrice = $"{relatedProduct.Price:N2} {currency}";
                    }
                }
            }
            else
            {
                product.FormattedPrice = $"{product.Price:N2} TRY";
                
                foreach (var relatedProduct in product.RelatedProducts)
                {
                    relatedProduct.FormattedPrice = $"{relatedProduct.Price:N2} TRY";
                }
            }
            
            product.ExchangeRates = await GetExchangeRatesAsync();
            product.SelectedCurrency = currency;
            
            return product;
        }

        public async Task<SearchResultViewModel> SearchProductsAsync(string query, int tenantId, string currency = "TRY", int page = 1, int pageSize = 12)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var offset = (page - 1) * pageSize;
            
            // Ürünleri ara
            var searchSql = @"
                SELECT 
                    p.ProductId, 
                    p.ProductName, 
                    p.CategoryId,
                    p.Price, 
                    p.ImagePath, 
                    c.CategoryName,
                    COUNT(*) OVER() as TotalCount
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.TenantId = @TenantId 
                    AND p.IsDeleted = 0
                    AND (
                        p.ProductName LIKE '%' + @Query + '%'
                        OR c.CategoryName LIKE '%' + @Query + '%'
                    )
                ORDER BY 
                    CASE WHEN p.ProductName LIKE @Query + '%' THEN 0
                         WHEN p.ProductName LIKE '%' + @Query + '%' THEN 1
                         ELSE 2
                    END,
                    p.ProductName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";
                
            var products = await connection.QueryAsync<ProductListItemViewModel>(
                searchSql,
                new { Query = query, TenantId = tenantId, Offset = offset, PageSize = pageSize }
            );
            
            var productList = products.ToList();
            var totalCount = productList.Any() ? productList.First().TotalCount : 0;
            
            // Döviz kuru dönüşümü
            if (currency != "TRY")
            {
                var rate = await _exchangeRateService.GetRateAsync(currency);
                if (rate != null)
                {
                    foreach (var product in productList)
                    {
                        var convertedPrice = await _exchangeRateService.ConvertCurrencyAsync(
                            product.Price, "TRY", currency);
                        product.Price = convertedPrice;
                        product.FormattedPrice = $"{convertedPrice:N2} {currency}";
                    }
                }
            }
            else
            {
                foreach (var product in productList)
                {
                    product.FormattedPrice = $"{product.Price:N2} TRY";
                }
            }
            
            // Kategorileri al
            var categories = await GetCategoriesAsync(tenantId);
            
            // Fiyat aralığını al
            var priceRangeSql = @"
                SELECT 
                    MIN(Price) as MinPrice,
                    MAX(Price) as MaxPrice
                FROM Products
                WHERE TenantId = @TenantId 
                    AND IsDeleted = 0
                    AND (
                        ProductName LIKE '%' + @Query + '%'
                        OR CategoryId IN (
                            SELECT CategoryId FROM Categories 
                            WHERE CategoryName LIKE '%' + @Query + '%'
                                AND TenantId = @TenantId
                        )
                    )";
                    
            var priceRange = await connection.QueryFirstOrDefaultAsync(
                priceRangeSql,
                new { Query = query, TenantId = tenantId }
            );
            
            return new SearchResultViewModel
            {
                SearchQuery = query,
                Products = productList,
                Categories = categories,
                MinPrice = priceRange?.MinPrice,
                MaxPrice = priceRange?.MaxPrice,
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalCount
                },
                ExchangeRates = await GetExchangeRatesAsync(),
                SelectedCurrency = currency
            };
        }

        public async Task<SearchResultViewModel> FilterProductsAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string[] properties, int tenantId, string currency = "TRY", int page = 1, int pageSize = 12)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var offset = (page - 1) * pageSize;
            
            // Dinamik SQL oluştur
            var whereClauses = new List<string>
            {
                "p.TenantId = @TenantId",
                "p.IsDeleted = 0"
            };
            
            var parameters = new DynamicParameters();
            parameters.Add("TenantId", tenantId);
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);
            
            if (categoryId.HasValue)
            {
                whereClauses.Add("p.CategoryId = @CategoryId");
                parameters.Add("CategoryId", categoryId.Value);
            }
            
            if (minPrice.HasValue)
            {
                whereClauses.Add("p.Price >= @MinPrice");
                parameters.Add("MinPrice", minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                whereClauses.Add("p.Price <= @MaxPrice");
                parameters.Add("MaxPrice", maxPrice.Value);
            }
            
            var whereClause = string.Join(" AND ", whereClauses);
            
            // Özellik filtreleri için JOIN ekle
            var joinClause = "";
            if (properties != null && properties.Length > 0)
            {
                joinClause = @"
                    INNER JOIN ProductProperties pp ON p.ProductId = pp.ProductId
                    INNER JOIN Properties prop ON pp.PropertyId = prop.PropertyId";
                    
                whereClause += " AND (";
                
                var propertyConditions = new List<string>();
                for (int i = 0; i < properties.Length; i++)
                {
                    propertyConditions.Add($"(prop.[Key] + ':' + prop.Value) = @Property{i}");
                    parameters.Add($"Property{i}", properties[i]);
                }
                
                whereClause += string.Join(" OR ", propertyConditions);
                whereClause += ")";
            }
            
            // Ürünleri filtrele
            var sql = $@"
                SELECT 
                    p.ProductId, 
                    p.ProductName, 
                    p.CategoryId,
                    p.Price, 
                    p.ImagePath, 
                    c.CategoryName,
                    COUNT(*) OVER() as TotalCount
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                {joinClause}
                WHERE {whereClause}
                GROUP BY p.ProductId, p.ProductName, p.CategoryId, p.Price, p.ImagePath, c.CategoryName
                ORDER BY p.ProductName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";
                
            var products = await connection.QueryAsync<ProductListItemViewModel>(sql, parameters);
            
            var productList = products.ToList();
            var totalCount = productList.Any() ? productList.First().TotalCount : 0;
            
            // Döviz kuru dönüşümü
            if (currency != "TRY")
            {
                var rate = await _exchangeRateService.GetRateAsync(currency);
                if (rate != null)
                {
                    foreach (var product in productList)
                    {
                        var convertedPrice = await _exchangeRateService.ConvertCurrencyAsync(
                            product.Price, "TRY", currency);
                        product.Price = convertedPrice;
                        product.FormattedPrice = $"{convertedPrice:N2} {currency}";
                    }
                }
            }
            else
            {
                foreach (var product in productList)
                {
                    product.FormattedPrice = $"{product.Price:N2} TRY";
                }
            }
            
            // Kategorileri al
            var categories = await GetCategoriesAsync(tenantId);
            
            // Mevcut filtreleri al
            var availableFiltersSql = @"
                SELECT DISTINCT
                    p.PropertyId,
                    p.[Key],
                    p.Value
                FROM Properties p
                INNER JOIN ProductProperties pp ON p.PropertyId = pp.PropertyId
                INNER JOIN Products prod ON pp.ProductId = prod.ProductId
                WHERE prod.TenantId = @TenantId 
                    AND prod.IsDeleted = 0
                    AND (@CategoryId IS NULL OR prod.CategoryId = @CategoryId)";
                    
            var availableFilters = await connection.QueryAsync<PropertyViewModel>(
                availableFiltersSql,
                new { TenantId = tenantId, CategoryId = categoryId }
            );
            
            return new SearchResultViewModel
            {
                Products = productList,
                Categories = categories,
                AvailableFilters = availableFilters.ToList(),
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalCount
                },
                ExchangeRates = await GetExchangeRatesAsync(),
                SelectedCurrency = currency
            };
        }

        private async Task<List<ProductListItemViewModel>> GetFeaturedProductsAsync(int tenantId, string currency = "TRY")
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = @"
                SELECT TOP 6
                    p.ProductId, 
                    p.ProductName, 
                    p.CategoryId,
                    p.Price, 
                    p.ImagePath, 
                    c.CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.TenantId = @TenantId 
                    AND p.IsDeleted = 0
                ORDER BY p.CreatedDate DESC";
                
            var products = await connection.QueryAsync<ProductListItemViewModel>(
                sql,
                new { TenantId = tenantId }
            );
            
            var productList = products.ToList();
            
            // Döviz kuru dönüşümü
            if (currency != "TRY")
            {
                var rate = await _exchangeRateService.GetRateAsync(currency);
                if (rate != null)
                {
                    foreach (var product in productList)
                    {
                        var convertedPrice = await _exchangeRateService.ConvertCurrencyAsync(
                            product.Price, "TRY", currency);
                        product.Price = convertedPrice;
                        product.FormattedPrice = $"{convertedPrice:N2} {currency}";
                    }
                }
            }
            else
            {
                foreach (var product in productList)
                {
                    product.FormattedPrice = $"{product.Price:N2} TRY";
                }
            }
            
            return productList;
        }

        private async Task<List<ProductListItemViewModel>> GetNewProductsAsync(int tenantId, string currency = "TRY")
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = @"
                SELECT TOP 6
                    p.ProductId, 
                    p.ProductName, 
                    p.CategoryId,
                    p.Price, 
                    p.ImagePath, 
                    c.CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.TenantId = @TenantId 
                    AND p.IsDeleted = 0
                ORDER BY p.CreatedDate DESC";
                
            var products = await connection.QueryAsync<ProductListItemViewModel>(
                sql,
                new { TenantId = tenantId }
            );
            
            var productList = products.ToList();
            
            // Döviz kuru dönüşümü
            if (currency != "TRY")
            {
                var rate = await _exchangeRateService.GetRateAsync(currency);
                if (rate != null)
                {
                    foreach (var product in productList)
                    {
                        var convertedPrice = await _exchangeRateService.ConvertCurrencyAsync(
                            product.Price, "TRY", currency);
                        product.Price = convertedPrice;
                        product.FormattedPrice = $"{convertedPrice:N2} {currency}";
                    }
                }
            }
            else
            {
                foreach (var product in productList)
                {
                    product.FormattedPrice = $"{product.Price:N2} TRY";
                }
            }
            
            return productList;
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
        {
            var rates = await _exchangeRateService.GetCurrentRatesAsync();
            return rates.ToDictionary(r => r.CurrencyCode, r => r.Rate);
        }
    }
} 