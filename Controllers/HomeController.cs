using CafeMenu.Models;
using CafeMenu.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CafeMenu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IExchangeRateService _exchangeRateService;

        public HomeController(ICategoryService categoryService, IProductService productService, IExchangeRateService exchangeRateService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _exchangeRateService = exchangeRateService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetCategoryHierarchyAsync();
            return View(categories);
        }

        public async Task<IActionResult> Category(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            decimal DolarKuru = await _exchangeRateService.GetDollarRateAsync();
            var products = await _productService.GetProductsByCategoryIdAsync(id, DolarKuru);
            ViewBag.Category = category;
            return View(products);
        }

        public async Task<IActionResult> Product(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}