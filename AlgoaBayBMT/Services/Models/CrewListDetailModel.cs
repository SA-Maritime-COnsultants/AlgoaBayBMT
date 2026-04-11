namespace AlgoaBayBMT.Services.Models
{
    public sealed class CrewListDetailModel
    {
        public int DeploymentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CellNo { get; set; }
        public string QualificationDisplay { get; set; } = string.Empty;
        public string OnboardRoleName { get; set; } = string.Empty;
        public string? SidNumber { get; set; }
        public DateTime? SidIssueDate { get; set; }
        public DateTime? SidExpiryDate { get; set; }
        public string? SidIssuingAuthority { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? PassportNumber { get; set; }
        public DateTime? PassportExpiry { get; set; }
        public string? EmbarkationPort { get; set; }
        public DateTime? EmbarkationDate { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? MedicalFitnessExpiry { get; set; }
        public string? VaccinationStatus { get; set; }
        public string VesselName { get; set; } = string.Empty;
        public int SeniorityOrder { get; set; }
    }
}
