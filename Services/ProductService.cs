using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CafeMenu.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CafeMenu.Services
{
    public class ProductService : IProductService
    {
        private readonly string _connectionString;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public ProductService(IConfiguration configuration, ICacheService cacheService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _cacheService = cacheService;
        }

        private string GetProductListCacheKey(int tenantId, int categoryId, int page) 
            => $"products_tenant_{tenantId}_category_{categoryId}_page_{page}";

        private string GetProductCountCacheKey(int tenantId, int categoryId) 
            => $"products_count_tenant_{tenantId}_category_{categoryId}";

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int tenantId, int categoryId, int page, int pageSize)
        {
            var cacheKey = GetProductListCacheKey(tenantId, categoryId, page);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var connection = new SqlConnection(_connectionString);
                var offset = (page - 1) * pageSize;

                var sql = @"
                    SELECT p.*, c.CategoryName
                    FROM Products p
                    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                    WHERE p.TenantId = @TenantId 
                    AND p.CategoryId = @CategoryId 
                    AND p.IsDeleted = 0
                    ORDER BY p.ProductId
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                var products = await connection.QueryAsync<Product>(sql, new 
                { 
                    TenantId = tenantId, 
                    CategoryId = categoryId,
                    Offset = offset,
                    PageSize = pageSize
                });

                return products;
            }, _cacheExpiration);
        }

        public async Task<int> GetTotalProductCountByCategoryAsync(int tenantId, int categoryId)
        {
            var cacheKey = GetProductCountCacheKey(tenantId, categoryId);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"
                    SELECT COUNT(*)
                    FROM Products
                    WHERE TenantId = @TenantId 
                    AND CategoryId = @CategoryId 
                    AND IsDeleted = 0";

                return await connection.ExecuteScalarAsync<int>(sql, new { TenantId = tenantId, CategoryId = categoryId });
            }, _cacheExpiration);
        }

        public async Task<Product> GetProductByIdAsync(int tenantId, int productId)
        {
            var cacheKey = $"product_tenant_{tenantId}_id_{productId}";

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"
                    SELECT p.*, c.CategoryName
                    FROM Products p
                    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                    WHERE p.TenantId = @TenantId 
                    AND p.ProductId = @ProductId 
                    AND p.IsDeleted = 0";

                return await connection.QueryFirstOrDefaultAsync<Product>(sql, 
                    new { TenantId = tenantId, ProductId = productId });
            }, _cacheExpiration);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Products (TenantId, ProductName, CategoryId, Price, ImagePath, CreatorUserId)
                VALUES (@TenantId, @ProductName, @CategoryId, @Price, @ImagePath, @CreatorUserId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            product.ProductId = await connection.ExecuteScalarAsync<int>(sql, product);
            await InvalidateProductCacheAsync(product.TenantId, product.CategoryId);
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Products 
                SET ProductName = @ProductName,
                    CategoryId = @CategoryId,
                    Price = @Price,
                    ImagePath = @ImagePath
                WHERE ProductId = @ProductId AND TenantId = @TenantId";

            await connection.ExecuteAsync(sql, product);
            await InvalidateProductCacheAsync(product.TenantId, product.CategoryId);
            return product;
        }

        public async Task<bool> DeleteProductAsync(int tenantId, int productId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Products 
                SET IsDeleted = 1
                WHERE ProductId = @ProductId AND TenantId = @TenantId;
                SELECT CategoryId FROM Products WHERE ProductId = @ProductId";

            var categoryId = await connection.ExecuteScalarAsync<int>(sql, new { ProductId = productId, TenantId = tenantId });
            await InvalidateProductCacheAsync(tenantId, categoryId);
            return true;
        }

        public async Task InvalidateProductCacheAsync(int tenantId, int categoryId)
        {
            // Sayfa bazlı önbellekleri temizle
            var tasks = new List<Task>();
            for (int page = 1; page <= 10; page++) // İlk 10 sayfayı temizle
            {
                var cacheKey = GetProductListCacheKey(tenantId, categoryId, page);
                tasks.Add(_cacheService.RemoveAsync(cacheKey));
            }

            // Toplam ürün sayısı önbelleğini temizle
            var countCacheKey = GetProductCountCacheKey(tenantId, categoryId);
            tasks.Add(_cacheService.RemoveAsync(countCacheKey));

            await Task.WhenAll(tasks);
        }
    }
} 