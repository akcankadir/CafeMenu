using CafeMenu.Models;

namespace CafeMenu.Services
{
    public interface IExchangeRateService
    {
        Task<decimal> GetDollarRateAsync();
    }
} 