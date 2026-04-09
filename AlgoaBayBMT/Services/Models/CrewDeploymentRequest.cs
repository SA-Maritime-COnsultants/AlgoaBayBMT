using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models
{
    public class CrewDeploymentRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int VesselId { get; set; }
        public string OnboardRoleName { get; set; } = string.Empty;
        public VesselRoleType OnboardRoleType { get; set; }
        public string? DeployedByUserId { get; set; }
        public DateTime StartedOnUtc { get; set; } = DateTime.UtcNow;
        public List<CrewDeploymentComplianceSnapshotInput> ComplianceSnapshots { get; set; } = new();
    }

    public class CrewDeploymentComplianceSnapshotInput
    {
        public string ComplianceItemName { get; set; } = string.Empty;
        public ComplianceState ComplianceState { get; set; }
        public DateTime? ExpiresOnUtc { get; set; }
    }
}
