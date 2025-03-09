using System.Threading.Tasks;
using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface ITenantService
    {
        Task<Tenant> GetTenantByDomainAsync(string domain);
        Task<Tenant> GetTenantByIdAsync(int tenantId);
        Task<bool> IsTenantActiveAsync(int tenantId);
    }
} 