using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using CafeMenu.Models;
using Microsoft.AspNetCore.SignalR;
using CafeMenu.Hubs;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Linq;

namespace CafeMenu.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
        private readonly ICacheService _cacheService;
        private readonly IHubContext<ExchangeRateHub> _hubContext;
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly string _apiUrl;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
        private readonly AsyncRetryPolicy _retryPolicy;

        public ExchangeRateService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ICacheService cacheService,
            IHubContext<ExchangeRateHub> hubContext,
            ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _cacheService = cacheService;
            _hubContext = hubContext;
            _logger = logger;
            _apiUrl = configuration["ExchangeRateApi:Url"];
            
            // Retry politikası oluştur
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<JsonException>()
                .WaitAndRetryAsync(
                    3, // 3 kez dene
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Döviz kuru çekme denemesi {retryCount} başarısız oldu. Hata: {exception.Message}. {timeSpan.TotalSeconds} saniye sonra tekrar deneniyor.");
                    });
        }

        public async Task<IEnumerable<ExchangeRate>> GetCurrentRatesAsync()
        {
            const string cacheKey = "current_exchange_rates";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                try
                {
                    using var connection = new SqlConnection(_connectionString);
                    var sql = "SELECT * FROM ExchangeRates WHERE UpdateDate > DATEADD(HOUR, -1, GETDATE()) ORDER BY CurrencyCode";
                    var rates = await connection.QueryAsync<ExchangeRate>(sql);
                    
                    if (rates.Any())
                    {
                        return rates;
                    }
                    
                    // Veritabanında güncel kur yoksa API'den çek
                    return await FetchRatesFromApiAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Döviz kurları alınırken hata oluştu.");
                    return await GetFallbackRatesAsync();
                }
            }, _cacheExpiration);
        }

        public async Task<ExchangeRate> GetRateAsync(string currencyCode)
        {
            var rates = await GetCurrentRatesAsync();
            return rates.FirstOrDefault(r => r.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
        }

        public async Task UpdateRatesAsync()
        {
            try
            {
                var rates = await FetchRatesFromApiAsync();
                
                if (rates.Any())
                {
                    await SaveRatesToDatabaseAsync(rates);
                    await _hubContext.Clients.All.SendAsync("ReceiveExchangeRates", rates);
                    
                    // Cache'i güncelle
                    await _cacheService.SetAsync("current_exchange_rates", rates, _cacheExpiration);
                    
                    _logger.LogInformation($"{rates.Count()} döviz kuru başarıyla güncellendi.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları güncellenirken hata oluştu.");
            }
        }

        private async Task<IEnumerable<ExchangeRate>> FetchRatesFromApiAsync()
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var rates = JsonSerializer.Deserialize<List<ExchangeRate>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (rates != null)
                {
                    foreach (var rate in rates)
                    {
                        if (rate != null)
                        {
                            rate.UpdateDate = DateTime.Now;
                        }
                    }
                }
                
                return rates ?? new List<ExchangeRate>();
            });
        }

        private async Task SaveRatesToDatabaseAsync(IEnumerable<ExchangeRate> rates)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction();
            
            try
            {
                foreach (var rate in rates)
                {
                    var sql = @"
                        MERGE INTO ExchangeRates AS target
                        USING (SELECT @CurrencyCode AS CurrencyCode) AS source
                        ON (target.CurrencyCode = source.CurrencyCode)
                        WHEN MATCHED THEN
                            UPDATE SET Rate = @Rate, UpdateDate = @UpdateDate
                        WHEN NOT MATCHED THEN
                            INSERT (CurrencyCode, Rate, UpdateDate)
                            VALUES (@CurrencyCode, @Rate, @UpdateDate);";
                    
                    await connection.ExecuteAsync(sql, rate, transaction);
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task<IEnumerable<ExchangeRate>> GetFallbackRatesAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = "SELECT * FROM ExchangeRates ORDER BY UpdateDate DESC, CurrencyCode";
                return await connection.QueryAsync<ExchangeRate>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback döviz kurları alınırken hata oluştu.");
                
                // Son çare olarak sabit kurlar döndür
                return new List<ExchangeRate>
                {
                    new ExchangeRate { CurrencyCode = "USD", Rate = 8.5m, UpdateDate = DateTime.Now },
                    new ExchangeRate { CurrencyCode = "EUR", Rate = 10.0m, UpdateDate = DateTime.Now },
                    new ExchangeRate { CurrencyCode = "GBP", Rate = 11.5m, UpdateDate = DateTime.Now }
                };
            }
        }

        public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            var fromRate = await GetRateAsync(fromCurrency);
            var toRate = await GetRateAsync(toCurrency);

            if (fromRate == null || toRate == null)
                throw new ArgumentException("Geçersiz para birimi");

            // TRY'ye çevir, sonra hedef para birimine
            var tryAmount = amount * fromRate.Rate;
            return tryAmount / toRate.Rate;
        }
    }
} 