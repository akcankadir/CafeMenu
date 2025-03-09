using System.Collections.Generic;
using System.Threading.Tasks;
using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface IProductCacheService
    {
        Task<Product> GetProductAsync(int productId, int tenantId);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int tenantId, int page = 1, int pageSize = 50);
        Task<List<Product>> GetAllProductsAsync(int tenantId, int page = 1, int pageSize = 50);
        Task CacheProductAsync(Product product, int tenantId);
        Task CacheProductsAsync(IEnumerable<Product> products, int tenantId);
        Task RemoveProductFromCacheAsync(int productId, int tenantId);
        Task InvalidateCategoryProductsAsync(int categoryId, int tenantId);
        Task WarmupCacheAsync(int tenantId);
    }
} 