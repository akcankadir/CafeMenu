using System.Collections.Generic;
using System.Threading.Tasks;
using CafeMenu.Models;
using CafeMenu.Models.ViewModels;

namespace CafeMenu.Services
{
    public interface ICustomerService
    {
        Task<CustomerHomeViewModel> GetHomePageDataAsync(int tenantId, string currency = "TRY");
        Task<CategoryViewModel> GetCategoryProductsAsync(int categoryId, int tenantId, string currency = "TRY", int page = 1, int pageSize = 12);
        Task<ProductDetailViewModel> GetProductDetailsAsync(int productId, int tenantId, string currency = "TRY");
        Task<SearchResultViewModel> SearchProductsAsync(string query, int tenantId, string currency = "TRY", int page = 1, int pageSize = 12);
        Task<SearchResultViewModel> FilterProductsAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string[] properties, int tenantId, string currency = "TRY", int page = 1, int pageSize = 12);
    }
} 