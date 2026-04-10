namespace AlgoaBayBMT.Services.Models
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 25;
        public bool UseSsl { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string? CompanyName { get; set; }
        public string? LogoUrl { get; set; }
        public string? SupportEmail { get; set; }
    }
}
