using System.Collections.Generic;
using System.Threading.Tasks;
using CafeMenu.Models.ViewModels;

namespace CafeMenu.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(int tenantId);
        Task<List<CategoryStatViewModel>> GetCategoryStatsAsync(int tenantId);
    }
} 