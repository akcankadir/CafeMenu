using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CafeMenu.Data;
using CafeMenu.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CafeMenu.Services
{
    /// <summary>
    /// Tenant (kiracı) işlemlerini yöneten servis sınıfı.
    /// Multi-tenant mimarinin temel bileşenidir.
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TenantService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// TenantService sınıfının constructor metodu.
        /// </summary>
        /// <param name="serviceProvider">Servis sağlayıcı</param>
        /// <param name="cache">Bellek önbelleği</param>
        /// <param name="logger">Loglama servisi</param>
        public TenantService(
            IServiceProvider serviceProvider, 
            IMemoryCache cache,
            ILogger<TenantService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Domain adına göre tenant bilgisini getirir.
        /// </summary>
        /// <param name="domain">Tenant domain adresi</param>
        /// <returns>Tenant nesnesi veya null</returns>
        public async Task<Tenant> GetTenantByDomainAsync(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("GetTenantByDomainAsync çağrısı boş domain ile yapıldı.");
                return null;
            }

            var cacheKey = $"tenant_domain_{domain.ToLower()}";
            
            // Önce cache'den kontrol et
            if (_cache.TryGetValue(cacheKey, out Tenant tenant))
            {
                _logger.LogDebug("Tenant {Domain} cache'den alındı.", domain);
                return tenant;
            }

            try
            {
                // DbContext'i scope içinde oluştur
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    tenant = await dbContext.Tenants
                        .AsNoTracking() // Performans için
                        .FirstOrDefaultAsync(t => t.Domain.ToLower() == domain.ToLower() && t.IsActive);

                    if (tenant != null)
                    {
                        _logger.LogInformation("Tenant bulundu: {TenantId} - {TenantName}", tenant.TenantId, tenant.Name);
                        
                        // Cache'e ekle
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(_cacheDuration)
                            .SetPriority(CacheItemPriority.High);

                        _cache.Set(cacheKey, tenant, cacheOptions);
                    }
                    else
                    {
                        _logger.LogWarning("Tenant bulunamadı: {Domain}", domain);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTenantByDomainAsync metodu çalışırken hata oluştu. Domain: {Domain}", domain);
                // Hata durumunda null dön, üst katmanda işlenecek
            }

            return tenant;
        }

        /// <summary>
        /// Tenant ID'sine göre tenant bilgisini getirir.
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>Tenant nesnesi veya null</returns>
        public async Task<Tenant> GetTenantByIdAsync(int tenantId)
        {
            if (tenantId <= 0)
            {
                _logger.LogWarning("GetTenantByIdAsync çağrısı geçersiz ID ile yapıldı: {TenantId}", tenantId);
                return null;
            }

            var cacheKey = $"tenant_id_{tenantId}";
            
            // Önce cache'den kontrol et
            if (_cache.TryGetValue(cacheKey, out Tenant tenant))
            {
                _logger.LogDebug("Tenant {TenantId} cache'den alındı.", tenantId);
                return tenant;
            }

            try
            {
                // DbContext'i scope içinde oluştur
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    tenant = await dbContext.Tenants
                        .AsNoTracking() // Performans için
                        .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsActive);

                    if (tenant != null)
                    {
                        _logger.LogInformation("Tenant bulundu: {TenantId} - {TenantName}", tenant.TenantId, tenant.Name);
                        
                        // Cache'e ekle
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(_cacheDuration)
                            .SetPriority(CacheItemPriority.High);

                        _cache.Set(cacheKey, tenant, cacheOptions);
                    }
                    else
                    {
                        _logger.LogWarning("Tenant bulunamadı: {TenantId}", tenantId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTenantByIdAsync metodu çalışırken hata oluştu. TenantId: {TenantId}", tenantId);
                // Hata durumunda null dön, üst katmanda işlenecek
            }

            return tenant;
        }

        /// <summary>
        /// Tenant'ın aktif olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>Tenant aktif ise true, değilse false</returns>
        public async Task<bool> IsTenantActiveAsync(int tenantId)
        {
            if (tenantId <= 0)
            {
                _logger.LogWarning("IsTenantActiveAsync çağrısı geçersiz ID ile yapıldı: {TenantId}", tenantId);
                return false;
            }

            try
            {
                var tenant = await GetTenantByIdAsync(tenantId);
                return tenant?.IsActive ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IsTenantActiveAsync metodu çalışırken hata oluştu. TenantId: {TenantId}", tenantId);
                return false;
            }
        }
    }
} 