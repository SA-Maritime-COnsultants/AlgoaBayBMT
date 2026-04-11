namespace AlgoaBayBMT.Shared.Models
{
    public class CrewChangeHistory : AuditableEntity
    {
        public int Id { get; set; }
        public int VesselId { get; set; }
        public string? UserId { get; set; }
        public int? DeploymentId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string ChangedByUserId { get; set; } = string.Empty;
        public string ChangedByName { get; set; } = string.Empty;
        public string ChangedBySurname { get; set; } = string.Empty;
        public string ChangedByRank { get; set; } = string.Empty;
        public DateTime ChangedOnUtc { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public string? BeforeJson { get; set; }
        public string? AfterJson { get; set; }
    }
}
