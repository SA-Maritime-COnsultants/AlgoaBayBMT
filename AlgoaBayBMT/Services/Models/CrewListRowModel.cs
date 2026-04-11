using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models
{
    public sealed class CrewListRowModel
    {
        public int DeploymentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OnboardRoleName { get; set; } = string.Empty;
        public VesselRoleType OnboardRoleType { get; set; }
        public string? EmbarkationPort { get; set; }
        public DateTime? EmbarkationDate { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? MedicalFitnessExpiry { get; set; }
        public string? VaccinationStatus { get; set; }
        public string? Duties { get; set; }
        public ComplianceState ComplianceState { get; set; }
        public DateTime? ComplianceExpiresOn { get; set; }
    }
}
