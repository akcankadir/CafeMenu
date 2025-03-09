using System.Collections.Generic;
using CafeMenu.Models;

namespace CafeMenu.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<CategoryStatViewModel> CategoryStats { get; set; } = new List<CategoryStatViewModel>();
        public List<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public int UserCount { get; set; }
        public List<ProductViewModel> RecentProducts { get; set; } = new List<ProductViewModel>();
        public List<PopularCategoryViewModel> PopularCategories { get; set; } = new List<PopularCategoryViewModel>();
        public List<PriceRangeStatViewModel> PriceRangeStats { get; set; } = new List<PriceRangeStatViewModel>();
    }

    public class CategoryStatViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public List<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();
    }

    public class PopularCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class PriceRangeStatViewModel
    {
        public string RangeLabel { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int Percentage { get; set; }
    }
} 