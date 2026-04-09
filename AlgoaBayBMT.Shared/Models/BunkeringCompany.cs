namespace AlgoaBayBMT.Shared.Models
{
    public class BunkeringCompany : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<CompanyAreaAssignment> AreaAssignments { get; set; } = new List<CompanyAreaAssignment>();
        public ICollection<Vessel> Vessels { get; set; } = new List<Vessel>();
        public ICollection<VesselRoleAssignment> VesselRoleAssignments { get; set; } = new List<VesselRoleAssignment>();
    }
}
