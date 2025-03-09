using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CafeMenu.Models;
using CafeMenu.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using CafeMenu.Data;

namespace CafeMenu.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IProductCacheService _productCacheService;

        public DashboardService(
            ApplicationDbContext context,
            IExchangeRateService exchangeRateService,
            IProductCacheService productCacheService)
        {
            _context = context;
            _exchangeRateService = exchangeRateService;
            _productCacheService = productCacheService;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(int tenantId)
        {
            var model = new DashboardViewModel
            {
                CategoryStats = await GetCategoryStatsAsync(tenantId),
                ExchangeRates = (await _exchangeRateService.GetCurrentRatesAsync()).ToList(),
                ProductCount = await _context.Products.CountAsync(p => !p.IsDeleted && p.TenantId == tenantId),
                CategoryCount = await _context.Categories.CountAsync(c => !c.IsDeleted && c.TenantId == tenantId),
                UserCount = await _context.Users.CountAsync(u => u.IsActive && u.TenantId == tenantId),
                RecentProducts = await GetRecentProductsAsync(tenantId),
                PopularCategories = await GetPopularCategoriesAsync(tenantId),
                PriceRangeStats = await GetPriceRangeStatsAsync(tenantId)
            };

            return model;
        }

        public async Task<List<CategoryStatViewModel>> GetCategoryStatsAsync(int tenantId)
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted && c.TenantId == tenantId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.CategoryId && !p.IsDeleted && p.TenantId == tenantId)
                })
                .ToListAsync();

            return categories
                .Select(c => new CategoryStatViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = c.ProductCount
                })
                .OrderByDescending(c => c.ProductCount)
                .ToList();
        }

        private async Task<List<ProductViewModel>> GetRecentProductsAsync(int tenantId)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    CategoryName = p.Category.CategoryName,
                    ImagePath = p.ImagePath
                })
                .ToListAsync();
        }

        private async Task<List<PopularCategoryViewModel>> GetPopularCategoriesAsync(int tenantId)
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted && c.TenantId == tenantId)
                .Select(c => new PopularCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.CategoryId && !p.IsDeleted && p.TenantId == tenantId),
                    AveragePrice = _context.Products
                        .Where(p => p.CategoryId == c.CategoryId && !p.IsDeleted && p.TenantId == tenantId)
                        .Average(p => (decimal?)p.Price) ?? 0
                })
                .OrderByDescending(c => c.ProductCount)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<PriceRangeStatViewModel>> GetPriceRangeStatsAsync(int tenantId)
        {
            var products = await _context.Products
                .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                .Select(p => p.Price)
                .ToListAsync();

            if (!products.Any())
            {
                return new List<PriceRangeStatViewModel>();
            }

            var minPrice = products.Min();
            var maxPrice = products.Max();
            var range = (maxPrice - minPrice) / 5; // 5 aralık oluştur

            var priceRanges = new List<PriceRangeStatViewModel>();

            for (int i = 0; i < 5; i++)
            {
                var lowerBound = minPrice + (range * i);
                var upperBound = (i == 4) ? maxPrice : minPrice + (range * (i + 1));
                
                var count = products.Count(p => p >= lowerBound && p <= upperBound);
                
                priceRanges.Add(new PriceRangeStatViewModel
                {
                    RangeLabel = $"{lowerBound:C2} - {upperBound:C2}",
                    ProductCount = count,
                    Percentage = (int)Math.Round((double)count / products.Count * 100)
                });
            }

            return priceRanges;
        }
    }
} 