namespace AlgoaBayBMT.Shared.Models
{
    public class AuthorityContact : AuditableEntity
    {
        public int Id { get; set; }
        public string AuthorityRole { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? UserId { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<AuthorityAreaAssignment> AreaAssignments { get; set; } = new List<AuthorityAreaAssignment>();
        public ICollection<AreaNotificationDistributionRule> DistributionRules { get; set; } = new List<AreaNotificationDistributionRule>();
    }
}
