using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CafeMenu.Services
{
    public class ExchangeRateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExchangeRateBackgroundService> _logger;
        private readonly TimeSpan _interval;

        public ExchangeRateBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ExchangeRateBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // Yapılandırmadan güncelleme aralığını al, varsayılan 10 saniye
            var seconds = configuration.GetValue<int>("ExchangeRateApi:UpdateIntervalSeconds", 10);
            _interval = TimeSpan.FromSeconds(seconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Döviz kuru güncelleme servisi başlatıldı. Güncelleme aralığı: {Interval} saniye.", _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateExchangeRatesAsync();
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Servis durdurulduğunda normal bir şekilde çık
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Döviz kurları güncellenirken bir hata oluştu.");
                    // Hata durumunda biraz bekle ve tekrar dene
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            
            _logger.LogInformation("Döviz kuru güncelleme servisi durduruldu.");
        }

        private async Task UpdateExchangeRatesAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
                await exchangeRateService.UpdateRatesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları güncellenirken bir hata oluştu.");
                throw;
            }
        }
    }
} 