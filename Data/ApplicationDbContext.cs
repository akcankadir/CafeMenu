using CafeMenu.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CafeMenu.Data
{
    /// <summary>
    /// Veritabanı bağlam sınıfı.
    /// Multi-tenant mimariye uygun olarak tasarlanmıştır.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly int? _tenantId;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApplicationDbContext> _logger;

        /// <summary>
        /// ApplicationDbContext constructor metodu.
        /// </summary>
        /// <param name="options">DbContext options</param>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        /// <param name="logger">Loglama servisi</param>
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // HttpContext'ten tenant bilgisini al
            _tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"] as int?;
            
            if (_tenantId.HasValue)
            {
                _logger.LogDebug("ApplicationDbContext oluşturuldu. TenantId: {TenantId}", _tenantId.Value);
            }
            else
            {
                _logger.LogWarning("ApplicationDbContext oluşturuldu, ancak TenantId bulunamadı.");
            }
        }

        // Entity setleri
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<ProductProperty> ProductProperties { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Model oluşturma sırasında ilişkileri ve filtreleri yapılandırır.
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _logger.LogDebug("OnModelCreating çağrıldı. Tenant filtreleri uygulanıyor.");

            // Tenant bazlı filtreleme
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted && c.TenantId == _tenantId);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted && p.TenantId == _tenantId);
            modelBuilder.Entity<Property>().HasQueryFilter(p => p.TenantId == _tenantId);
            modelBuilder.Entity<ProductProperty>().HasQueryFilter(pp => pp.TenantId == _tenantId);
            modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantId);

            // Ürün-Özellik ilişkisi
            modelBuilder.Entity<ProductProperty>()
                .HasKey(pp => new { pp.ProductId, pp.PropertyId });

            modelBuilder.Entity<ProductProperty>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductProperties)
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductProperty>()
                .HasOne(pp => pp.Property)
                .WithMany(p => p.ProductProperties)
                .HasForeignKey(pp => pp.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanıcı-Rol ilişkisi
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // İndeksler
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.TenantId, c.CategoryName })
                .IsUnique();
                
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.ProductName });
                
            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.TenantId, u.Username })
                .IsUnique();
                
            modelBuilder.Entity<Role>()
                .HasIndex(r => new { r.TenantId, r.Name })
                .IsUnique()
                .HasFilter("TenantId IS NOT NULL");
        }

        /// <summary>
        /// Değişiklikleri kaydederken tenant bilgisini otomatik olarak ekler.
        /// </summary>
        /// <param name="cancellationToken">İptal token'ı</param>
        /// <returns>Etkilenen satır sayısı</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("SaveChangesAsync çağrıldı. Tenant bilgisi ekleniyor.");
                
                foreach (var entry in ChangeTracker.Entries<BaseEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.TenantId = _tenantId ?? 0;
                        
                        // Oluşturma tarihi ve kullanıcı bilgisi
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        
                        // Kullanıcı ID'sini HttpContext'ten al (varsa)
                        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
                        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdValue))
                        {
                            entry.Entity.CreatorUserId = userIdValue;
                        }
                    }
                }

                return base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveChangesAsync sırasında hata oluştu.");
                throw; // Hatayı yukarı fırlat
            }
        }
    }
} 