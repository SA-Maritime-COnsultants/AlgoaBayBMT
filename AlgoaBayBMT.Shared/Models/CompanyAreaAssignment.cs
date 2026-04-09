namespace AlgoaBayBMT.Shared.Models
{
    public class CompanyAreaAssignment : AuditableEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int OperationalAreaId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? AssignedByUserId { get; set; }
        public DateTime AssignedOnUtc { get; set; } = DateTime.UtcNow;

        public BunkeringCompany? Company { get; set; }
        public OperationalArea? OperationalArea { get; set; }
    }
}
