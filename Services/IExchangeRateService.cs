using System.Collections.Generic;
using System.Threading.Tasks;
using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface IExchangeRateService
    {
        Task<IEnumerable<ExchangeRate>> GetCurrentRatesAsync();
        Task<ExchangeRate> GetRateAsync(string currencyCode);
        Task UpdateRatesAsync();
        Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    }
} 