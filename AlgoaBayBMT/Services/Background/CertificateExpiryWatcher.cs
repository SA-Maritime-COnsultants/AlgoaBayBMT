using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services.Background
{
    /// <summary>
    /// Periodically scans crew documents and training certificates for upcoming expiries,
    /// queues notifications, and updates expiry-event statuses.
    /// </summary>
    public class CertificateExpiryWatcher(
        IServiceProvider serviceProvider,
        ILogger<CertificateExpiryWatcher> logger,
        IConfiguration configuration) : BackgroundService
    {
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromHours(6);
        private static readonly int[] ReminderThresholdsDays = [60, 30, 14, 7, 1];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = configuration.GetValue<TimeSpan?>("Crewing:ExpiryScanInterval") ?? DefaultInterval;
            var horizonDays = configuration.GetValue<int?>("Crewing:ExpiryHorizonDays") ?? 60;

            // Initial delay so the app starts up cleanly
            try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
            catch (TaskCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var compliance = scope.ServiceProvider.GetRequiredService<IComplianceService>();
                    var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

                    var newEvents = await compliance.ScanExpiriesAsync(horizonDays, stoppingToken);
                    if (newEvents > 0)
                    {
                        logger.LogInformation("CertificateExpiryWatcher detected {Count} new expiry events", newEvents);
                    }

                    await using var db = await dbFactory.CreateDbContextAsync(stoppingToken);
                    var pending = await db.CertificateExpiryEvents
                        .Include(e => e.CrewMember)
                        .Where(e => e.Status == ExpiryEventStatus.Pending || e.Status == ExpiryEventStatus.Notified)
                        .ToListAsync(stoppingToken);

                    var nowUtc = DateTime.UtcNow;
                    foreach (var ev in pending)
                    {
                        var daysLeft = (int)Math.Floor((ev.ExpiryDateUtc - nowUtc).TotalDays);
                        ev.DaysUntilExpiry = Math.Max(0, daysLeft);

                        if (daysLeft < 0)
                        {
                            ev.Status = ExpiryEventStatus.Expired;
                            await QueueExpiryNotification(notifications, ev, expired: true, stoppingToken);
                            continue;
                        }

                        // Notify on threshold crossings
                        if (ReminderThresholdsDays.Contains(daysLeft) && ev.Status == ExpiryEventStatus.Pending)
                        {
                            await QueueExpiryNotification(notifications, ev, expired: false, stoppingToken);
                            ev.Status = ExpiryEventStatus.Notified;
                            ev.NotifiedOnUtc = nowUtc;
                        }
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    logger.LogError(ex, "CertificateExpiryWatcher iteration failed");
                }

                try { await Task.Delay(interval, stoppingToken); }
                catch (TaskCanceledException) { break; }
            }
        }

        private static async Task QueueExpiryNotification(INotificationService notifications, CertificateExpiryEvent ev, bool expired, CancellationToken cancellationToken)
        {
            var crew = ev.CrewMember;
            if (crew is null || string.IsNullOrEmpty(crew.ApplicationUserId)) return;

            var subject = expired
                ? $"Certificate expired for {crew.FirstName} {crew.LastName}"
                : $"Certificate expiring in {ev.DaysUntilExpiry} day(s) for {crew.FirstName} {crew.LastName}";

            var body = $"Source: {ev.Source}. Expiry date: {ev.ExpiryDateUtc:dd MMM yyyy - HH:mm} UTC.";

            await notifications.QueueAsync(
                recipientUserId: crew.ApplicationUserId,
                recipientEmail: crew.Email,
                recipientDisplayName: $"{crew.FirstName} {crew.LastName}",
                subject: subject,
                body: body,
                category: NotificationCategory.CertificateExpiry,
                channel: string.IsNullOrEmpty(crew.Email) ? NotificationChannel.InApp : NotificationChannel.Both,
                relatedCrewMemberId: crew.Id,
                relatedExpiryEventId: ev.Id,
                cancellationToken: cancellationToken);
        }
    }
}
