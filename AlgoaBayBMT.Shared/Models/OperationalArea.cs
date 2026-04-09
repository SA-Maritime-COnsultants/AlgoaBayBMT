namespace AlgoaBayBMT.Shared.Models
{
    public class OperationalArea : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Port> Ports { get; set; } = new List<Port>();
        public ICollection<Bay> Bays { get; set; } = new List<Bay>();
        public ICollection<Anchorage> Anchorages { get; set; } = new List<Anchorage>();
        public ICollection<UserAreaAssignment> UserAssignments { get; set; } = new List<UserAreaAssignment>();
        public ICollection<CompanyAreaAssignment> CompanyAssignments { get; set; } = new List<CompanyAreaAssignment>();
        public ICollection<AuthorityAreaAssignment> AuthorityAssignments { get; set; } = new List<AuthorityAreaAssignment>();
        public ICollection<AreaNotificationDistributionRule> NotificationDistributionRules { get; set; } = new List<AreaNotificationDistributionRule>();
    }
}
