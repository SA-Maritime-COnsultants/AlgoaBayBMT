using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services.Background
{
    /// <summary>
    /// Periodically re-evaluates compliance for every vessel that currently has signed-on crew,
    /// producing a fresh VesselComplianceSnapshot.
    /// </summary>
    public class ComplianceRevalidator(
        IServiceProvider serviceProvider,
        ILogger<ComplianceRevalidator> logger,
        IConfiguration configuration) : BackgroundService
    {
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromHours(12);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = configuration.GetValue<TimeSpan?>("Crewing:ComplianceRevalidationInterval") ?? DefaultInterval;

            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
            catch (TaskCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
                    var compliance = scope.ServiceProvider.GetRequiredService<IComplianceService>();

                    await using var db = await dbFactory.CreateDbContextAsync(stoppingToken);
                    var vesselIds = await db.CrewAssignments
                        .Where(a => a.Status == CrewAssignmentStatus.SignedOn)
                        .Select(a => a.VesselId)
                        .Distinct()
                        .ToListAsync(stoppingToken);

                    foreach (var vesselId in vesselIds)
                    {
                        try
                        {
                            await compliance.EvaluateVesselAsync(vesselId, performedByUserId: null, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Revalidation failed for vessel {VesselId}", vesselId);
                        }
                    }
                    if (vesselIds.Count > 0)
                    {
                        logger.LogInformation("ComplianceRevalidator processed {Count} vessels", vesselIds.Count);
                    }
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ComplianceRevalidator iteration failed");
                }

                try { await Task.Delay(interval, stoppingToken); }
                catch (TaskCanceledException) { break; }
            }
        }
    }
}
