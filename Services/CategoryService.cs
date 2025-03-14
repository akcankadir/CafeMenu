using CafeMenu.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CafeMenu.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly string _connectionString;

        public CategoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = new List<Category>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Category_GetAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                ParentCategoryId = reader.IsDBNull(reader.GetOrdinal("ParentCategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("ParentCategoryId")),
                                ParentCategoryName = reader.IsDBNull(reader.GetOrdinal("ParentCategoryName")) ? null : reader.GetString(reader.GetOrdinal("ParentCategoryName")),
                                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                CreatedUserId = reader.GetInt32(reader.GetOrdinal("CreatedUserId"))
                            });
                        }
                    }
                }
            }

            return categories;
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            Category? category = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Category_GetById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CategoryId", categoryId);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            category = new Category
                            {
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                ParentCategoryId = reader.IsDBNull(reader.GetOrdinal("ParentCategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("ParentCategoryId")),
                                ParentCategoryName = reader.IsDBNull(reader.GetOrdinal("ParentCategoryName")) ? null : reader.GetString(reader.GetOrdinal("ParentCategoryName")),
                                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                CreatedUserId = reader.GetInt32(reader.GetOrdinal("CreatedUserId"))
                            };
                        }
                    }
                }
            }

            return category;
        }

        public async Task<int> CreateCategoryAsync(Category category)
        {
            int categoryId = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Category_Insert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    command.Parameters.AddWithValue("@ParentCategoryId", category.ParentCategoryId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedUserId", category.CreatedUserId);

                    await connection.OpenAsync();

                    var result = await command.ExecuteScalarAsync();
                    if (result != null && int.TryParse(result.ToString(), out int id))
                    {
                        categoryId = id;
                    }
                }
            }

            return categoryId;
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Category_Update", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CategoryId", category.CategoryId);
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    command.Parameters.AddWithValue("@ParentCategoryId", category.ParentCategoryId ?? (object)DBNull.Value);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_Category_Delete", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CategoryId", categoryId);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Category>> GetCategoryHierarchyAsync()
        {
            var allCategories = await GetAllCategoriesAsync();
            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();

            foreach (var rootCategory in rootCategories)
            {
                rootCategory.SubCategories = BuildSubcategories(allCategories, rootCategory.CategoryId);
            }

            return rootCategories;
        }

        private List<Category> BuildSubcategories(List<Category> allCategories, int parentId)
        {
            var subcategories = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();

            foreach (var subcategory in subcategories)
            {
                subcategory.SubCategories = BuildSubcategories(allCategories, subcategory.CategoryId);
            }

            return subcategories;
        }
    }
} 