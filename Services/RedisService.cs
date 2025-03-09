using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace CafeMenu.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly string _instanceName;

        public RedisService(IDistributedCache cache, IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _cache = cache;
            _redis = redis;
            var connectionString = configuration.GetConnectionString("Redis");
            _instanceName = configuration["Redis:InstanceName"];
            _db = _redis.GetDatabase();
        }

        private string CreateKey(string key) => $"{_instanceName}:{key}";

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(value))
                return default;

            return JsonConvert.DeserializeObject<T>(value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiry.Value;

            var serializedValue = JsonConvert.SerializeObject(value);
            await _cache.SetStringAsync(key, serializedValue, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(CreateKey(key));
        }

        public async Task<List<T>> GetListAsync<T>(string pattern)
        {
            var result = new List<T>();
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            
            await foreach (var key in server.KeysAsync(pattern: CreateKey(pattern)))
            {
                var value = await GetAsync<T>(key);
                if (value != null)
                    result.Add(value);
            }

            return result;
        }

        public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).Select(k => k.ToString()).ToList();
            return keys;
        }
        
        public async Task<Dictionary<string, T>> GetMultipleAsync<T>(IEnumerable<string> keys)
        {
            var result = new Dictionary<string, T>();
            var db = _redis.GetDatabase();
            
            var tasks = keys.Select(async key => 
            {
                var value = await db.StringGetAsync(key);
                if (!value.IsNull)
                {
                    result[key] = JsonConvert.DeserializeObject<T>(value);
                }
            });
            
            await Task.WhenAll(tasks);
            return result;
        }
        
        public async Task SetMultipleAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiry = null)
        {
            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            
            foreach (var kv in keyValues)
            {
                var serializedValue = JsonConvert.SerializeObject(kv.Value);
                batch.StringSetAsync(kv.Key, serializedValue, expiry);
            }
            
            batch.Execute();
        }
        
        public async Task RemoveMultipleAsync(IEnumerable<string> keys)
        {
            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            
            foreach (var key in keys)
            {
                batch.KeyDeleteAsync(key);
            }
            
            batch.Execute();
        }
    }
} 