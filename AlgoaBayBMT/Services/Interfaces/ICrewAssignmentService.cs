using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICrewAssignmentService
    {
        Task<List<CrewAssignment>> GetActiveByVesselAsync(int vesselId, CancellationToken cancellationToken = default);
        Task<List<CrewAssignment>> GetByCrewMemberAsync(int crewMemberId, CancellationToken cancellationToken = default);
        Task<CrewAssignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to sign a crew member onto a vessel. Compliance is evaluated first;
        /// non-compliant crew are rejected with an explanatory result.
        /// </summary>
        Task<CrewAssignmentResult> SignOnAsync(int crewMemberId, int vesselId, CrewRank rankOnAssignment, string? portOfSignOn, string? performedByUserId, CancellationToken cancellationToken = default);

        Task<CrewAssignment> SignOffAsync(int assignmentId, string? portOfSignOff, string? performedByUserId, CancellationToken cancellationToken = default);
        Task CancelAsync(int assignmentId, string? performedByUserId, CancellationToken cancellationToken = default);
    }

    public class CrewAssignmentResult
    {
        public bool Succeeded { get; set; }
        public CrewAssignment? Assignment { get; set; }
        public ComplianceResult? ComplianceResult { get; set; }
        public string? Message { get; set; }
    }
}
