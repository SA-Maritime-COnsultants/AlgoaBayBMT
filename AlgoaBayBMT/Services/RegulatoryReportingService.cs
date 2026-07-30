using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class RegulatoryReportingService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IRegulatoryReportingService
    {
        public async Task<List<VesselComplianceReportRow>> GetVesselComplianceReportAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.VesselComplianceSnapshots.AsNoTracking()
                .Include(s => s.Vessel)
                .Where(s => s.EvaluatedOnUtc >= fromUtc && s.EvaluatedOnUtc <= toUtc)
                .OrderByDescending(s => s.EvaluatedOnUtc)
                .Select(s => new VesselComplianceReportRow
                {
                    VesselId = s.VesselId,
                    VesselName = s.Vessel!.Name,
                    EvaluatedOnUtc = s.EvaluatedOnUtc,
                    State = s.State,
                    CompliantCrewCount = s.CompliantCrewCount,
                    NonCompliantCrewCount = s.NonCompliantCrewCount,
                    ExpiringSoonCount = s.ExpiringSoonCount,
                    TotalCrewCount = s.TotalCrewCount
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ExpiringCertificateReportRow>> GetExpiringCertificatesReportAsync(int horizonDays, CancellationToken cancellationToken = default)
        {
            if (horizonDays <= 0) horizonDays = 60;
            var horizonUtc = DateTime.UtcNow.AddDays(horizonDays);
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await db.CertificateExpiryEvents.AsNoTracking()
                .Include(e => e.CrewMember)
                .Include(e => e.CrewDocument)
                .Include(e => e.TrainingCertificate)
                .Where(e => e.ExpiryDateUtc <= horizonUtc && e.Status != ExpiryEventStatus.Acknowledged)
                .OrderBy(e => e.ExpiryDateUtc)
                .Select(e => new ExpiringCertificateReportRow
                {
                    CrewMemberId = e.CrewMemberId,
                    CrewMemberName = e.CrewMember!.LastName + ", " + e.CrewMember.FirstName,
                    Source = e.Source,
                    ItemTitle = e.Source == ExpirySource.CrewDocument
                        ? (e.CrewDocument != null ? e.CrewDocument.Title : null)
                        : (e.TrainingCertificate != null ? e.TrainingCertificate.CertificateNumber : null),
                    ExpiryDateUtc = e.ExpiryDateUtc,
                    DaysUntilExpiry = e.DaysUntilExpiry,
                    Status = e.Status
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ComplianceAuditEntry>> GetAuditTrailAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.ComplianceAuditEntries.AsNoTracking()
                .Where(a => a.OccurredOnUtc >= fromUtc && a.OccurredOnUtc <= toUtc)
                .OrderByDescending(a => a.OccurredOnUtc)
                .ToListAsync(cancellationToken);
        }
    }
}
