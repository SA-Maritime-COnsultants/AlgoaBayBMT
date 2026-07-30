using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IComplianceService
    {
        /// <summary>
        /// Evaluates compliance for a crew member, optionally in the context of a specific vessel/rank
        /// (used for sign-on checks). Persists a ComplianceResult and returns it.
        /// </summary>
        Task<ComplianceResult> EvaluateCrewMemberAsync(int crewMemberId, int? vesselId, CrewRank? rankContext, string? performedByUserId, CancellationToken cancellationToken = default);

        Task<VesselComplianceSnapshot> EvaluateVesselAsync(int vesselId, string? performedByUserId, CancellationToken cancellationToken = default);

        Task<List<ComplianceRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Scans all crew certificates / documents and creates CertificateExpiryEvent rows
        /// for items expiring within the configured horizon. Returns the number of newly tracked events.
        /// </summary>
        Task<int> ScanExpiriesAsync(int horizonDays = 60, CancellationToken cancellationToken = default);
    }
}
