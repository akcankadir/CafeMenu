using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CafeMenu.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        public RedisCacheService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("RedisConnection");
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _cache = _redis.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _cache.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.StringSetAsync(key, serializedValue, expirationTime ?? _defaultExpiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _cache.KeyExistsAsync(key);
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null)
        {
            var value = await GetAsync<T>(key);
            if (value != null)
                return value;

            value = await factory();
            await SetAsync(key, value, expirationTime);
            return value;
        }
    }
} 