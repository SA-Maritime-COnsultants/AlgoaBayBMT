namespace AlgoaBayBMT.Shared.Models
{
    public class CrewDeployment : AuditableEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int VesselId { get; set; }
        public string OnboardRoleName { get; set; } = string.Empty;
        public VesselRoleType OnboardRoleType { get; set; }
        public DeploymentStatus Status { get; set; } = DeploymentStatus.Active;
        public DateTime StartedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? EndedOnUtc { get; set; }
        public string? DeployedByUserId { get; set; }
        public string? Notes { get; set; }
        public string? EmbarkationPort { get; set; }
        public string? DisembarkationPort { get; set; }
        public string? DisembarkationReason { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? MedicalFitnessExpiry { get; set; }
        public string? VaccinationStatus { get; set; }
        public string? Duties { get; set; }

        public Vessel? Vessel { get; set; }
        public ICollection<CrewDeploymentComplianceSnapshot> ComplianceSnapshots { get; set; } = new List<CrewDeploymentComplianceSnapshot>();
        public ICollection<VesselCrewListEntry> CrewListEntries { get; set; } = new List<VesselCrewListEntry>();
    }
}
