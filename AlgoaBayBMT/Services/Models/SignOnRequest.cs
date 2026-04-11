using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models
{
    public sealed class SignOnRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int VesselId { get; set; }
        public SeagoingCommercialQualification? Qualification { get; set; }
        public OnBoardRoles? OnBoardRole { get; set; }
        public string OnboardRoleName { get; set; } = string.Empty;
        public VesselRoleType OnboardRoleType { get; set; }
        public string? Duties { get; set; }
        public string EmbarkationPort { get; set; } = string.Empty;
        public DateTime EmbarkationDate { get; set; } = DateTime.UtcNow;
        public DateTime ContractStartDate { get; set; } = DateTime.UtcNow;
        public DateTime ContractEndDate { get; set; } = DateTime.UtcNow.AddMonths(6);
        public DateTime? MedicalFitnessExpiry { get; set; }
        public string? VaccinationStatus { get; set; }
        public string? Notes { get; set; }
        public string DeployedByUserId { get; set; } = string.Empty;
    }
}
