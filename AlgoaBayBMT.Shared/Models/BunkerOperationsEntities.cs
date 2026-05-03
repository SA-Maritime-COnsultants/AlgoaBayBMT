using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Shared.Models
{
    public class AreaOfOperation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? RegionCode { get; set; }

        [StringLength(100)]
        public string? EnvironmentalSensitivityRating { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Port> Ports { get; set; } = new List<Port>();
        public ICollection<OperatorAreaAssignment> OperatorAssignments { get; set; } = new List<OperatorAreaAssignment>();
        public ICollection<BargeDeployment> BargeDeployments { get; set; } = new List<BargeDeployment>();
    }

    public class Port
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "Port";

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Depth { get; set; }

        [StringLength(100)]
        public string? MaxVesselSize { get; set; }

        public bool IsAnchorage { get; set; }
        public bool IsBunkeringAllowed { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public int AreaOfOperationId { get; set; }
        public AreaOfOperation AreaOfOperation { get; set; } = null!;
    }

    public class BunkerOperator
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CompanyRegistrationNumber { get; set; }

        [StringLength(500)]
        public string? PhysicalAddress { get; set; }

        [StringLength(150)]
        public string? ContactPerson { get; set; }

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(50)]
        public string? Phone { get; set; }

        [Phone]
        [StringLength(50)]
        public string? EmergencyContactNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<OperatorAreaAssignment> AreaAssignments { get; set; } = new List<OperatorAreaAssignment>();
        public ICollection<BunkerBarge> Barges { get; set; } = new List<BunkerBarge>();
    }

    public class OperatorAreaAssignment
    {
        public int Id { get; set; }

        public int BunkerOperatorId { get; set; }
        public BunkerOperator BunkerOperator { get; set; } = null!;

        public int AreaOfOperationId { get; set; }
        public AreaOfOperation AreaOfOperation { get; set; } = null!;

        public DateTime AssignedFrom { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedTo { get; set; }
    }

    public class BunkerBarge
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? IMO { get; set; }

        [StringLength(20)]
        public string? MMSI { get; set; }

        [StringLength(30)]
        public string? CallSign { get; set; }

        public double? CapacityMT { get; set; }

        [StringLength(300)]
        public string? FuelTypesSupported { get; set; }

        public double? PumpingRate { get; set; }
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? CertificationExpiry { get; set; }
        public int? CrewCapacity { get; set; }
        public bool IsActive { get; set; } = true;

        public int BunkerOperatorId { get; set; }
        public BunkerOperator BunkerOperator { get; set; } = null!;

        public ICollection<BargeDeployment> Deployments { get; set; } = new List<BargeDeployment>();
    }

    public class BargeDeployment
    {
        public int Id { get; set; }

        public int BunkerBargeId { get; set; }
        public BunkerBarge BunkerBarge { get; set; } = null!;

        public int AreaOfOperationId { get; set; }
        public AreaOfOperation AreaOfOperation { get; set; } = null!;

        public DateTime DeployedFrom { get; set; } = DateTime.UtcNow;
        public DateTime? DeployedTo { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public ICollection<BargeDeploymentAudit> AuditEntries { get; set; } = new List<BargeDeploymentAudit>();
    }

    public class BargeDeploymentAudit
    {
        public int Id { get; set; }

        public int BargeDeploymentId { get; set; }
        public BargeDeployment Deployment { get; set; } = null!;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string PerformedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Details { get; set; } = string.Empty;
    }
}
