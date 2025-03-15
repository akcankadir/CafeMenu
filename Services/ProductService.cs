using CafeMenu.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CafeMenu.Services
{
    public class ProductService : IProductService
    {
        private readonly string _connectionString;

        public ProductService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Product_GetAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
                                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                CreatedUserId = reader.GetInt32(reader.GetOrdinal("CreatedUserId"))
                            });
                        }
                    }
                }
            }

            return products;
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            Product? product = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Product_GetById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", productId);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            product = new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
                                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                CreatedUserId = reader.GetInt32(reader.GetOrdinal("CreatedUserId"))
                            };
                        }
                    }
                }
            }

            return product;
        }

        public async Task<List<ProductUS>> GetProductsByCategoryIdAsync(int categoryId,decimal DolarKuru)
        {
            var products = new List<ProductUS>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Product_GetByCategoryId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CategoryId", categoryId);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new ProductUS
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
                                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                CreatedUserId = reader.GetInt32(reader.GetOrdinal("CreatedUserId")),
                                USPrice = reader.GetDecimal(reader.GetOrdinal("Price")) / DolarKuru
                            });
                        }
                    }
                }
            }

            return products;
        }

        public async Task<int> CreateProductAsync(Product product)
        {
            int productId = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Product_Insert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@ImagePath", product.ImagePath ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedUserId", product.CreatedUserId);

                    await connection.OpenAsync();

                    var result = await command.ExecuteScalarAsync();
                    if (result != null && int.TryParse(result.ToString(), out int id))
                    {
                        productId = id;
                    }
                }
            }

            return productId;
        }

        public async Task UpdateProductAsync(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Product_Update", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", product.ProductId);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@ImagePath", product.ImagePath ?? (object)DBNull.Value);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Product_Delete", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", productId);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 