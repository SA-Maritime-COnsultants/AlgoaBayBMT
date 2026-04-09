namespace AlgoaBayBMT.Shared.Models
{
    public class UserAreaAssignment : AuditableEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int OperationalAreaId { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
        public string? AssignedByUserId { get; set; }
        public DateTime AssignedOnUtc { get; set; } = DateTime.UtcNow;

        public OperationalArea? OperationalArea { get; set; }
    }
}
