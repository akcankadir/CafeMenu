using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CafeMenu.Services
{
    public interface IRedisService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);
        Task<Dictionary<string, T>> GetMultipleAsync<T>(IEnumerable<string> keys);
        Task SetMultipleAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiry = null);
        Task RemoveMultipleAsync(IEnumerable<string> keys);
    }
} 