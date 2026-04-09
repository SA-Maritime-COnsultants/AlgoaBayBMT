namespace AlgoaBayBMT.Services.Models
{
    public class NotificationRecipient
    {
        public string RoleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? AuthorityContactId { get; set; }
        public int? CompanyId { get; set; }
        public string? UserId { get; set; }
    }
}
