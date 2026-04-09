namespace AlgoaBayBMT.Shared.Models
{
    public class VesselRoleAssignment : AuditableEntity
    {
        public int Id { get; set; }
        public int VesselId { get; set; }
        public int? CompanyId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public VesselRoleType VesselRoleType { get; set; }
        public DateTime AssignedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? EndedOnUtc { get; set; }
        public string? AssignedByUserId { get; set; }
        public bool IsActive { get; set; } = true;

        public Vessel? Vessel { get; set; }
        public BunkeringCompany? Company { get; set; }
    }
}
