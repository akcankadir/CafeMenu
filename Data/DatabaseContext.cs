using Microsoft.EntityFrameworkCore;
using CafeMenu.Models;

namespace CafeMenu.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<ProductProperty> ProductProperties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User tablosu
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<User>().HasKey(u => u.UserId);

            // Category tablosu
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
            modelBuilder.Entity<Category>().Ignore(c => c.SubCategories);
            modelBuilder.Entity<Category>().Ignore(c => c.Products);
            modelBuilder.Entity<Category>().Ignore(c => c.ParentCategoryName);

            // Product tablosu
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<Product>().HasKey(p => p.ProductId);
            modelBuilder.Entity<Product>().Ignore(p => p.ProductProperties);
            modelBuilder.Entity<Product>().Ignore(p => p.CategoryName);

            // Property tablosu
            modelBuilder.Entity<Property>().ToTable("Property");
            modelBuilder.Entity<Property>().HasKey(p => p.PropertyId);

            // ProductProperty tablosu
            modelBuilder.Entity<ProductProperty>().ToTable("ProductProperty");
            modelBuilder.Entity<ProductProperty>().HasKey(pp => pp.ProductPropertyId);
            modelBuilder.Entity<ProductProperty>().Ignore(pp => pp.ProductName);
            modelBuilder.Entity<ProductProperty>().Ignore(pp => pp.PropertyKey);
            modelBuilder.Entity<ProductProperty>().Ignore(pp => pp.PropertyValue);
        }
    }
} 