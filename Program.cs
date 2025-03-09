using CafeMenu.Data;
using CafeMenu.Services;
using CafeMenu.Hubs;
using CafeMenu.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// Servis Kayıtları
// ===================================================

// MVC ve View desteği
builder.Services.AddControllersWithViews();

// HttpContext erişimi için gerekli
builder.Services.AddHttpContextAccessor();

// Bellek Cache servisi
builder.Services.AddMemoryCache();

// ===================================================
// Veritabanı Yapılandırması
// ===================================================

// SQL Server bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlServerOptionsAction: sqlOptions =>
        {
            // Bağlantı hatalarında yeniden deneme politikası
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// ===================================================
// Multi-Tenancy Yapılandırması
// ===================================================

// Tenant servisi
builder.Services.AddScoped<ITenantService, TenantService>();

// ===================================================
// Önbellek Yapılandırması
// ===================================================

// Redis Cache servisi
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

// Redis servisleri
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddScoped<IProductCacheService, ProductCacheService>();

// ===================================================
// Kimlik Doğrulama ve Yetkilendirme
// ===================================================

// Cookie tabanlı kimlik doğrulama
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// ===================================================
// İş Mantığı Servisleri
// ===================================================

// Kullanıcı ve rol servisleri
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Redis Cache Servisi
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(connectionString);
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Ürünler için özel cache servisi
builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions
{
    SizeLimit = null // 5 milyon kayıt için sınırsız cache boyutu
}));

// ===================================================
// Gerçek Zamanlı İletişim
// ===================================================

// SignalR
builder.Services.AddSignalR();

// ===================================================
// Dış Servis Entegrasyonları
// ===================================================

// HTTP Client
builder.Services.AddHttpClient();

// Döviz kuru servisleri
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddHostedService<ExchangeRateBackgroundService>();

// ===================================================
// Rate Limiting (DDoS Koruması)
// ===================================================

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Çok fazla istek. Lütfen daha sonra tekrar deneyin.", token);
    };
});

// ===================================================
// CORS Politikası
// ===================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ===================================================
// Loglama Yapılandırması
// ===================================================

// Loglama servisleri
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Geliştirme ortamında daha detaylı loglar
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Belirli kategoriler için log seviyelerini ayarla
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("CafeMenu", LogLevel.Debug);

// ===================================================
// Uygulama Yapılandırması
// ===================================================

var app = builder.Build();

// Ortam yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Temel middleware'ler
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Güvenlik middleware'leri
app.UseCors("DefaultPolicy");
app.UseRateLimiter();
app.UseTenantMiddleware();
app.UseAuthentication();
app.UseAuthorization();

// SignalR hub endpoint'i
app.MapHub<ExchangeRateHub>("/exchangeRateHub");

// MVC route yapılandırması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulamayı başlat
app.Run();
