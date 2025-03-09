using System.Collections.Generic;
using System.Threading.Tasks;
using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int tenantId, int categoryId, int page, int pageSize);
        Task<int> GetTotalProductCountByCategoryAsync(int tenantId, int categoryId);
        Task<Product> GetProductByIdAsync(int tenantId, int productId);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int tenantId, int productId);
        Task InvalidateProductCacheAsync(int tenantId, int categoryId);
    }
} 