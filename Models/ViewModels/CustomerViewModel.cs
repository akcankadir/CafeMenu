using System.Collections.Generic;
using CafeMenu.Models;

namespace CafeMenu.Models.ViewModels
{
    public class CustomerHomeViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<ProductListItemViewModel> FeaturedProducts { get; set; } = new List<ProductListItemViewModel>();
        public List<ProductListItemViewModel> NewProducts { get; set; } = new List<ProductListItemViewModel>();
        public Dictionary<string, decimal> ExchangeRates { get; set; } = new Dictionary<string, decimal>();
        public string SelectedCurrency { get; set; } = "TRY";
    }
    
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public List<CategoryViewModel> SubCategories { get; set; } = new List<CategoryViewModel>();
        public List<ProductListItemViewModel> Products { get; set; } = new List<ProductListItemViewModel>();
        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
        public Dictionary<string, decimal> ExchangeRates { get; set; } = new Dictionary<string, decimal>();
        public string SelectedCurrency { get; set; } = "TRY";
    }
    
    public class ProductListItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
    
    public class ProductDetailViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();
        public List<ProductListItemViewModel> RelatedProducts { get; set; } = new List<ProductListItemViewModel>();
        public Dictionary<string, decimal> ExchangeRates { get; set; } = new Dictionary<string, decimal>();
        public string SelectedCurrency { get; set; } = "TRY";
    }
    
    public class PropertyViewModel
    {
        public int PropertyId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
    
    public class SearchResultViewModel
    {
        public string SearchQuery { get; set; } = string.Empty;
        public List<ProductListItemViewModel> Products { get; set; } = new List<ProductListItemViewModel>();
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<PropertyViewModel> AvailableFilters { get; set; } = new List<PropertyViewModel>();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
        public Dictionary<string, decimal> ExchangeRates { get; set; } = new Dictionary<string, decimal>();
        public string SelectedCurrency { get; set; } = "TRY";
    }
    
    public class PaginationInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (TotalItems + ItemsPerPage - 1) / ItemsPerPage;
    }
} 