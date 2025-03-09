using System.Threading.Tasks;
using System.Collections.Generic;
using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface IUserService
    {
        Task<User> ValidateUserAsync(string username, string password);
        Task<User> CreateUserAsync(User user, string password);
        Task<bool> UpdatePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UpdateUserRolesAsync(int userId, int[] roleIds);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<User> UpdateProfileAsync(User user);
        Task<bool> IsEmailUniqueAsync(string email, int? userId = null);
    }
} 