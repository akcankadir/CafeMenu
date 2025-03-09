using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CafeMenu.Services;
using CafeMenu.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace CafeMenu.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IExchangeRateService _exchangeRateService;

        public CustomerController(
            ICustomerService customerService,
            IExchangeRateService exchangeRateService)
        {
            _customerService = customerService;
            _exchangeRateService = exchangeRateService;
        }

        public async Task<IActionResult> Index(string currency = "TRY")
        {
            // Tenant ID'yi HttpContext'ten al
            var tenantId = HttpContext.Items.ContainsKey("TenantId") 
                ? (int)HttpContext.Items["TenantId"] 
                : 1; // Varsayılan tenant
                
            var model = await _customerService.GetHomePageDataAsync(tenantId, currency);
            return View(model);
        }

        public async Task<IActionResult> Category(int id, string currency = "TRY", int page = 1, int pageSize = 12)
        {
            // Tenant ID'yi HttpContext'ten al
            var tenantId = HttpContext.Items.ContainsKey("TenantId") 
                ? (int)HttpContext.Items["TenantId"] 
                : 1; // Varsayılan tenant
                
            var model = await _customerService.GetCategoryProductsAsync(id, tenantId, currency, page, pageSize);
            
            if (model == null)
            {
                return NotFound();
            }
            
            return View(model);
        }

        public async Task<IActionResult> Product(int id, string currency = "TRY")
        {
            // Tenant ID'yi HttpContext'ten al
            var tenantId = HttpContext.Items.ContainsKey("TenantId") 
                ? (int)HttpContext.Items["TenantId"] 
                : 1; // Varsayılan tenant
                
            var model = await _customerService.GetProductDetailsAsync(id, tenantId, currency);
            
            if (model == null)
            {
                return NotFound();
            }
            
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Search(string query, string currency = "TRY", int page = 1, int pageSize = 12)
        {
            // Tenant ID'yi HttpContext'ten al
            var tenantId = HttpContext.Items.ContainsKey("TenantId") 
                ? (int)HttpContext.Items["TenantId"] 
                : 1; // Varsayılan tenant
                
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }
            
            var model = await _customerService.SearchProductsAsync(query, tenantId, currency, page, pageSize);
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Filter(
            int? categoryId, 
            decimal? minPrice, 
            decimal? maxPrice, 
            string[] properties, 
            string currency = "TRY", 
            int page = 1, 
            int pageSize = 12)
        {
            // Tenant ID'yi HttpContext'ten al
            var tenantId = HttpContext.Items.ContainsKey("TenantId") 
                ? (int)HttpContext.Items["TenantId"] 
                : 1; // Varsayılan tenant
                
            var model = await _customerService.FilterProductsAsync(
                categoryId, 
                minPrice, 
                maxPrice, 
                properties, 
                tenantId, 
                currency, 
                page, 
                pageSize);
                
            return View("Search", model);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetExchangeRates()
        {
            var rates = await _exchangeRateService.GetCurrentRatesAsync();
            return Json(rates);
        }
    }
} 