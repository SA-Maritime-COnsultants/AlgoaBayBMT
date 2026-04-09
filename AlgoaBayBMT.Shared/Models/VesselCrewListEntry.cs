namespace AlgoaBayBMT.Shared.Models
{
    public class VesselCrewListEntry : AuditableEntity
    {
        public int Id { get; set; }
        public int VesselId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? CrewDeploymentId { get; set; }
        public string OnboardRoleName { get; set; } = string.Empty;
        public VesselRoleType OnboardRoleType { get; set; }
        public DateTime JoinedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LeftOnUtc { get; set; }
        public ComplianceState ComplianceState { get; set; } = ComplianceState.Unknown;
        public DateTime? ComplianceExpiresOnUtc { get; set; }
        public bool IsCurrent { get; set; } = true;

        public Vessel? Vessel { get; set; }
        public CrewDeployment? CrewDeployment { get; set; }
    }
}
