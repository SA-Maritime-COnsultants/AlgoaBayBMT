namespace AlgoaBayBMT.Shared.Models
{
    public class CrewDeploymentComplianceSnapshot : AuditableEntity
    {
        public int Id { get; set; }
        public int CrewDeploymentId { get; set; }
        public string ComplianceItemName { get; set; } = string.Empty;
        public ComplianceState ComplianceState { get; set; } = ComplianceState.Unknown;
        public DateTime? ExpiresOnUtc { get; set; }
        public DateTime RecordedOnUtc { get; set; } = DateTime.UtcNow;

        public CrewDeployment? CrewDeployment { get; set; }
    }
}
