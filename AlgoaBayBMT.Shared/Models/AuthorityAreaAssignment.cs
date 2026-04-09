namespace AlgoaBayBMT.Shared.Models
{
    public class AuthorityAreaAssignment : AuditableEntity
    {
        public int Id { get; set; }
        public int AuthorityContactId { get; set; }
        public int OperationalAreaId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? AssignedByUserId { get; set; }
        public DateTime AssignedOnUtc { get; set; } = DateTime.UtcNow;

        public AuthorityContact? AuthorityContact { get; set; }
        public OperationalArea? OperationalArea { get; set; }
    }
}
