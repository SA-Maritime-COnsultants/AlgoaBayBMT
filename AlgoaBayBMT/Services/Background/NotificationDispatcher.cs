using AlgoaBayBMT.Services.Interfaces;

namespace AlgoaBayBMT.Services.Background
{
    /// <summary>
    /// Periodically dispatches pending NotificationMessages (in-app + email).
    /// </summary>
    public class NotificationDispatcher(
        IServiceProvider serviceProvider,
        ILogger<NotificationDispatcher> logger,
        IConfiguration configuration) : BackgroundService
    {
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(2);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = configuration.GetValue<TimeSpan?>("Crewing:NotificationDispatchInterval") ?? DefaultInterval;
            var batchSize = configuration.GetValue<int?>("Crewing:NotificationBatchSize") ?? 50;

            try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
            catch (TaskCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var dispatched = await notifications.DispatchPendingAsync(batchSize, stoppingToken);
                    if (dispatched > 0)
                    {
                        logger.LogInformation("NotificationDispatcher sent {Count} notifications", dispatched);
                    }
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    logger.LogError(ex, "NotificationDispatcher iteration failed");
                }

                try { await Task.Delay(interval, stoppingToken); }
                catch (TaskCanceledException) { break; }
            }
        }
    }
}
