namespace AlgoaBayBMT.Shared.Models
{
    public class Vessel : AuditableEntity
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImoNumber { get; set; } = string.Empty;
        public string? CallSign { get; set; }
        public string? FlagState { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }

        public BunkeringCompany? Company { get; set; }
        public ICollection<VesselRoleAssignment> RoleAssignments { get; set; } = new List<VesselRoleAssignment>();
        public ICollection<CrewDeployment> CrewDeployments { get; set; } = new List<CrewDeployment>();
        public ICollection<VesselCrewListEntry> CrewListEntries { get; set; } = new List<VesselCrewListEntry>();
    }
}
