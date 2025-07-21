using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meal_Planning.Infrastructure.Services
{
    public class SubscriptionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionBackgroundService> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromHours(12); // Process twice daily

        public SubscriptionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SubscriptionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Subscription Background Service is processing subscriptions");

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var subscriptionService = scope.ServiceProvider.GetRequiredService<SubscriptionService>();
                        await subscriptionService.ProcessSubscriptionRenewals();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing subscription renewals");
                }

                _logger.LogInformation("Subscription Background Service is sleeping for {Interval} hours", 
                    _processingInterval.TotalHours);
                
                // Delay until next run time
                await Task.Delay(_processingInterval, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
