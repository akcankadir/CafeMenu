using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CafeMenu.Services;

namespace CafeMenu.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            // TODO: Gerçek tenant ID'yi al
            var tenantId = 1;
            var model = await _dashboardService.GetDashboardDataAsync(tenantId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryStats()
        {
            // TODO: Gerçek tenant ID'yi al
            var tenantId = 1;
            var stats = await _dashboardService.GetCategoryStatsAsync(tenantId);
            return Json(stats);
        }
    }
} 