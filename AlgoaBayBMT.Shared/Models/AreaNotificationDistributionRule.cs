namespace AlgoaBayBMT.Shared.Models
{
    public class AreaNotificationDistributionRule : AuditableEntity
    {
        public int Id { get; set; }
        public int OperationalAreaId { get; set; }
        public int? CompanyId { get; set; }
        public int? AuthorityContactId { get; set; }
        public string NotificationCategory { get; set; } = string.Empty;
        public string RecipientRoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public OperationalArea? OperationalArea { get; set; }
        public BunkeringCompany? Company { get; set; }
        public AuthorityContact? AuthorityContact { get; set; }
    }
}
