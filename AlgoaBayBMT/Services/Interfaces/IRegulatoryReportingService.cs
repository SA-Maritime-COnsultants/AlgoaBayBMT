using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IRegulatoryReportingService
    {
        Task<List<VesselComplianceReportRow>> GetVesselComplianceReportAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
        Task<List<ExpiringCertificateReportRow>> GetExpiringCertificatesReportAsync(int horizonDays, CancellationToken cancellationToken = default);
        Task<List<ComplianceAuditEntry>> GetAuditTrailAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    }

    public class VesselComplianceReportRow
    {
        public int VesselId { get; set; }
        public string VesselName { get; set; } = string.Empty;
        public DateTime EvaluatedOnUtc { get; set; }
        public VesselComplianceState State { get; set; }
        public int CompliantCrewCount { get; set; }
        public int NonCompliantCrewCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int TotalCrewCount { get; set; }
    }

    public class ExpiringCertificateReportRow
    {
        public int CrewMemberId { get; set; }
        public string CrewMemberName { get; set; } = string.Empty;
        public ExpirySource Source { get; set; }
        public string? ItemTitle { get; set; }
        public DateTime ExpiryDateUtc { get; set; }
        public int DaysUntilExpiry { get; set; }
        public ExpiryEventStatus Status { get; set; }
    }
}
