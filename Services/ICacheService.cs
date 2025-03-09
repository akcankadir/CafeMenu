using System;
using System.Threading.Tasks;

namespace CafeMenu.Services
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null);
    }
} 