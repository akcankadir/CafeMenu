using CafeMenu.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CafeMenu.Services
{

    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;

        public ExchangeRateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetDollarRateAsync()
        {
            string apiUrl = "https://api.exchangerate-api.com/v4/latest/USD";
            var response = await _httpClient.GetStringAsync(apiUrl);

            if (string.IsNullOrEmpty(response))
            {
                throw new Exception("API'den dönen cevap boş.");
            }

            try
            {
                // JSON cevabını deseralize et
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var exchangeRateData = JsonSerializer.Deserialize<ExchangeRateResponse>(response, options);

                Console.WriteLine($"Deserializasyon başarılı: {exchangeRateData != null}");
                if (exchangeRateData != null)
                {
                    Console.WriteLine($"Provider: {exchangeRateData.Provider}");
                    Console.WriteLine($"Base: {exchangeRateData.Base}");
                    Console.WriteLine($"Date: {exchangeRateData.Date}");
                    Console.WriteLine($"Rates mevcut: {exchangeRateData.Rates != null}");
                }

                if (exchangeRateData == null || exchangeRateData.Rates == null || !exchangeRateData.Rates.ContainsKey("TRY"))
                {
                    throw new Exception("TRY kuru verisi bulunamadı.");
                }

                // Rates anahtarını yazdırarak hangi dövizlerin mevcut olduğunu görebilirsiniz
                foreach (var rate in exchangeRateData.Rates)
                {
                    Console.WriteLine($"{rate.Key}: {rate.Value}");
                }

                // TRY kuru değerini döndür
                return exchangeRateData.Rates["TRY"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                throw;
            }
        }

        public class ExchangeRateResponse
        {
            [JsonPropertyName("provider")]
            public string Provider { get; set; }
            
            [JsonPropertyName("WARNING_UPGRADE_TO_V6")]
            public string WARNING_UPGRADE_TO_V6 { get; set; }
            
            [JsonPropertyName("terms")]
            public string Terms { get; set; }
            
            [JsonPropertyName("base")]
            public string Base { get; set; }
            
            [JsonPropertyName("date")]
            public string Date { get; set; }
            
            [JsonPropertyName("time_last_updated")]
            public long Time_Last_Updated { get; set; }
            
            [JsonPropertyName("rates")]
            public Dictionary<string, decimal> Rates { get; set; }
        }

    }
}