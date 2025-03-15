using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<List<ProductUS>> GetProductsByCategoryIdAsync(int categoryId,decimal DolarKuru);
        Task<int> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int productId);
    }
} 