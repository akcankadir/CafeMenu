using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CafeMenu.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CafeMenu.Data;
using System.Collections.Concurrent;

namespace CafeMenu.Services
{
    public class ProductCacheService : IProductCacheService
    {
        private readonly IRedisService _redisService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks;
        
        // Tenant bazlı cache key'leri
        private const string PRODUCT_KEY = "tenant:{0}:product:{1}";
        private const string CATEGORY_PRODUCTS_KEY = "tenant:{0}:category:{1}:products";
        private const string ALL_PRODUCTS_KEY = "tenant:{0}:all:products";
        private const string PRODUCT_PARTITION_KEY = "tenant:{0}:products:partition:{1}";
        private const int PARTITION_SIZE = 1000; // Her partition'da 1000 ürün

        public ProductCacheService(
            IRedisService redisService,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _redisService = redisService;
            _context = context;
            _configuration = configuration;
            _locks = new ConcurrentDictionary<int, SemaphoreSlim>();
        }

        public async Task<Product> GetProductAsync(int productId, int tenantId)
        {
            var key = string.Format(PRODUCT_KEY, tenantId, productId);
            var product = await _redisService.GetAsync<Product>(key);

            if (product == null)
            {
                // Double-checked locking pattern
                var lockObj = _locks.GetOrAdd(productId, _ => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync();
                try
                {
                    // Tekrar kontrol et
                    product = await _redisService.GetAsync<Product>(key);
                    if (product == null)
                    {
                        product = await _context.Products
                            .Where(p => p.ProductId == productId && !p.IsDeleted && p.TenantId == tenantId)
                            .FirstOrDefaultAsync();
                            
                        if (product != null)
                        {
                            await CacheProductAsync(product, tenantId);
                        }
                    }
                }
                finally
                {
                    lockObj.Release();
                }
            }

            return product;
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int tenantId, int page = 1, int pageSize = 50)
        {
            var key = string.Format(CATEGORY_PRODUCTS_KEY, tenantId, categoryId);
            var cacheKey = $"{key}:page:{page}:size:{pageSize}";
            
            var products = await _redisService.GetAsync<List<Product>>(cacheKey);

            if (products == null)
            {
                products = await _context.Products
                    .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.TenantId == tenantId)
                    .OrderBy(p => p.ProductName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (products.Any())
                {
                    var expiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:ExpirationMinutes"));
                    await _redisService.SetAsync(cacheKey, products, expiry);
                    
                    // Arka planda tüm ürünleri partition'lara cache'le
                    _ = Task.Run(() => CacheProductsInPartitionsAsync(categoryId, tenantId));
                }
            }

            return products ?? new List<Product>();
        }

        public async Task<List<Product>> GetAllProductsAsync(int tenantId, int page = 1, int pageSize = 50)
        {
            var baseKey = string.Format(ALL_PRODUCTS_KEY, tenantId);
            var cacheKey = $"{baseKey}:page:{page}:size:{pageSize}";
            
            var products = await _redisService.GetAsync<List<Product>>(cacheKey);

            if (products == null)
            {
                products = await _context.Products
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                    .OrderBy(p => p.ProductName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (products.Any())
                {
                    var expiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:ExpirationMinutes"));
                    await _redisService.SetAsync(cacheKey, products, expiry);
                }
            }

            return products ?? new List<Product>();
        }

        public async Task CacheProductAsync(Product product, int tenantId)
        {
            if (product == null) return;

            var key = string.Format(PRODUCT_KEY, tenantId, product.ProductId);
            var expiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:ExpirationMinutes"));
            await _redisService.SetAsync(key, product, expiry);
            
            // Partition key'ini güncelle
            var partitionId = product.ProductId / PARTITION_SIZE;
            var partitionKey = string.Format(PRODUCT_PARTITION_KEY, tenantId, partitionId);
            
            // Partition'ı al, yoksa oluştur
            var partition = await _redisService.GetAsync<List<int>>(partitionKey) ?? new List<int>();
            
            if (!partition.Contains(product.ProductId))
            {
                partition.Add(product.ProductId);
                await _redisService.SetAsync(partitionKey, partition, expiry);
            }
        }

        public async Task CacheProductsAsync(IEnumerable<Product> products, int tenantId)
        {
            if (products == null) return;

            var tasks = products.Select(p => CacheProductAsync(p, tenantId));
            await Task.WhenAll(tasks);
        }

        private async Task CacheProductsInPartitionsAsync(int categoryId, int tenantId)
        {
            // Kategori için tüm ürün ID'lerini al
            var productIds = await _context.Products
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.TenantId == tenantId)
                .Select(p => p.ProductId)
                .ToListAsync();
                
            // Partition'lara böl
            var partitions = productIds
                .GroupBy(id => id / PARTITION_SIZE)
                .ToDictionary(g => g.Key, g => g.ToList());
                
            // Her partition'ı cache'le
            var expiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:ExpirationMinutes"));
            foreach (var partition in partitions)
            {
                var partitionKey = string.Format(PRODUCT_PARTITION_KEY, tenantId, partition.Key);
                await _redisService.SetAsync(partitionKey, partition.Value, expiry);
            }
        }

        public async Task RemoveProductFromCacheAsync(int productId, int tenantId)
        {
            var key = string.Format(PRODUCT_KEY, tenantId, productId);
            await _redisService.RemoveAsync(key);

            // Kategori cache'ini de temizle
            var product = await _context.Products.FindAsync(productId);
            if (product != null && product.TenantId == tenantId)
            {
                await InvalidateCategoryProductsAsync(product.CategoryId, tenantId);
                
                // Partition'dan da kaldır
                var partitionId = productId / PARTITION_SIZE;
                var partitionKey = string.Format(PRODUCT_PARTITION_KEY, tenantId, partitionId);
                
                var partition = await _redisService.GetAsync<List<int>>(partitionKey);
                if (partition != null && partition.Contains(productId))
                {
                    partition.Remove(productId);
                    if (partition.Any())
                    {
                        var expiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:ExpirationMinutes"));
                        await _redisService.SetAsync(partitionKey, partition, expiry);
                    }
                    else
                    {
                        await _redisService.RemoveAsync(partitionKey);
                    }
                }
            }

            // Tüm ürünler cache'ini temizle
            var baseKey = string.Format(ALL_PRODUCTS_KEY, tenantId);
            var keys = await _redisService.GetKeysByPatternAsync($"{baseKey}*");
            foreach (var cacheKey in keys)
            {
                await _redisService.RemoveAsync(cacheKey);
            }
        }

        public async Task InvalidateCategoryProductsAsync(int categoryId, int tenantId)
        {
            var key = string.Format(CATEGORY_PRODUCTS_KEY, tenantId, categoryId);
            var keys = await _redisService.GetKeysByPatternAsync($"{key}*");
            foreach (var cacheKey in keys)
            {
                await _redisService.RemoveAsync(cacheKey);
            }
        }
        
        public async Task WarmupCacheAsync(int tenantId)
        {
            // En popüler kategorileri cache'le
            var popularCategoryIds = await _context.Products
                .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                .GroupBy(p => p.CategoryId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();
                
            foreach (var categoryId in popularCategoryIds)
            {
                await GetProductsByCategoryAsync(categoryId, tenantId);
            }
            
            // İlk sayfayı cache'le
            await GetAllProductsAsync(tenantId);
        }
    }
} 