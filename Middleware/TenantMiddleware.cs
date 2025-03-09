using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using CafeMenu.Services;
using CafeMenu.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CafeMenu.Middleware
{
    /// <summary>
    /// Multi-tenant mimarinin temel bileşeni olan middleware.
    /// Her HTTP isteğinde tenant bilgisini belirler ve HttpContext'e ekler.
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        /// <summary>
        /// TenantMiddleware constructor metodu.
        /// </summary>
        /// <param name="next">Sonraki middleware</param>
        /// <param name="logger">Loglama servisi</param>
        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Middleware işlem metodu.
        /// </summary>
        /// <param name="context">HTTP context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var host = context.Request.Host.Host.ToLower();

            _logger.LogDebug("TenantMiddleware çalışıyor. Host: {Host}, Path: {Path}", host, requestPath);

            try
            {
                // Host adresinden tenant belirleme
                var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                var tenant = await tenantService.GetTenantByDomainAsync(host);

                if (tenant == null && !IsAdminPath(requestPath))
                {
                    _logger.LogWarning("Tenant bulunamadı ve admin yolu değil. Host: {Host}, Path: {Path}", host, requestPath);
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Tenant bulunamadı.");
                    return;
                }

                // Tenant bilgisini HttpContext'e ekle
                if (tenant != null)
                {
                    _logger.LogInformation("Tenant belirlendi: {TenantId} - {TenantName}", tenant.TenantId, tenant.Name);
                    context.Items["TenantId"] = tenant.TenantId;
                    context.Items["TenantName"] = tenant.Name;
                }
                else
                {
                    // Admin yolları için varsayılan tenant ID (0 veya başka bir değer)
                    _logger.LogInformation("Admin yolu için varsayılan tenant kullanılıyor. Path: {Path}", requestPath);
                    context.Items["TenantId"] = 0;
                    context.Items["TenantName"] = "Admin";
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TenantMiddleware işlemi sırasında hata oluştu. Host: {Host}, Path: {Path}", host, requestPath);
                
                // Hata durumunda 500 Internal Server Error döndür
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Sunucu hatası oluştu.");
                }
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogDebug("TenantMiddleware tamamlandı. Süre: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// İstek yolunun admin yolu olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="path">İstek yolu</param>
        /// <returns>Admin yolu ise true, değilse false</returns>
        private bool IsAdminPath(PathString path)
        {
            return path.StartsWithSegments("/Admin") || 
                   path.StartsWithSegments("/Account") || 
                   path.StartsWithSegments("/api/admin");
        }
    }

    /// <summary>
    /// TenantMiddleware için extension metotları.
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        /// <summary>
        /// TenantMiddleware'i uygulama pipeline'ına ekler.
        /// </summary>
        /// <param name="builder">Uygulama builder</param>
        /// <returns>Uygulama builder</returns>
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
} 