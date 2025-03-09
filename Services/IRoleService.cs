using System.Threading.Tasks;
using System.Collections.Generic;
using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int roleId);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<bool> IsRoleNameUniqueAsync(string roleName, int? roleId = null);
    }
} 