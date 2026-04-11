namespace AlgoaBayBMT.Services.Models
{
    public sealed class SignOffRequest
    {
        public int DeploymentId { get; set; }
        public string DisembarkationPort { get; set; } = string.Empty;
        public DateTime DisembarkationDate { get; set; } = DateTime.UtcNow;
        public string? DisembarkationReason { get; set; }
        public string? Notes { get; set; }
        public string SignedOffByUserId { get; set; } = string.Empty;
    }
}
