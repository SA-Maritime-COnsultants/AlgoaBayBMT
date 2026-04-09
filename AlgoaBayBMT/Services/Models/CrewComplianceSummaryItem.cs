using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models
{
    public class CrewComplianceSummaryItem
    {
        public string UserId { get; set; } = string.Empty;
        public string CrewMemberName { get; set; } = string.Empty;
        public int VesselId { get; set; }
        public string VesselName { get; set; } = string.Empty;
        public string OnboardRoleName { get; set; } = string.Empty;
        public ComplianceState ComplianceState { get; set; }
        public DateTime? ComplianceExpiresOnUtc { get; set; }
    }
}
