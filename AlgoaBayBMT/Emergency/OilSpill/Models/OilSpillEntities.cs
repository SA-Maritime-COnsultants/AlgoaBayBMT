using System.ComponentModel.DataAnnotations;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Emergency.OilSpill.Models
{
    public class OilSpillIncident
    {
        public int Id { get; set; }

        // FK to existing BunkeringOperation (optional - a spill may be reported outside a bunkering job)
        public int? BunkeringOperationId { get; set; }

        [Required]
        [StringLength(200)]
        public string SpillName { get; set; } = string.Empty;

        [Required]
        public DateTime SpillStartTime { get; set; }

        public DateTime? SpillEndTime { get; set; }

        [Required]
        public OilSpillProductType ProductType { get; set; }

        [Required]
        public double EstimatedVolume { get; set; }

        public double? ReleaseRate { get; set; }

        [Required]
        public OilSpillSourceType SourceType { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public OilSpillStatus Status { get; set; } = OilSpillStatus.Active;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>Incident Commander (IMS/ICS) responsible for the response; surfaced in SITREP metadata.</summary>
        [StringLength(150)]
        public string? Commander { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public BunkeringOperation? Operation { get; set; }
        public ICollection<OilSpillModelRun> ModelRuns { get; set; } = new List<OilSpillModelRun>();
        public ICollection<OilSpillResponseAction> ResponseActions { get; set; } = new List<OilSpillResponseAction>();
        public ICollection<IncidentForm> Forms { get; set; } = new List<IncidentForm>();
    }

    public class OilSpillModelRun
    {
        public int Id { get; set; }

        [Required]
        public int SpillId { get; set; }

        [Required]
        [StringLength(200)]
        public string RunName { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public double DurationHours { get; set; }

        [Required]
        public int TimeStepMinutes { get; set; }

        [Required]
        public double WindSpeed { get; set; }

        [Required]
        public double WindDirection { get; set; }

        [Required]
        public double CurrentSpeed { get; set; }

        [Required]
        public double CurrentDirection { get; set; }

        [Required]
        public OilSpillTideState TideState { get; set; }

        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Shoreline impact results (populated when the slick reaches the coastline during playback).
        public int? ShorelineImpactIndex { get; set; }
        public DateTime? ShorelineImpactTime { get; set; }

        // Navigation
        public OilSpillIncident Spill { get; set; } = null!;
        public ICollection<OilSpillTrajectoryPoint> TrajectoryPoints { get; set; } = new List<OilSpillTrajectoryPoint>();
    }

    public class OilSpillTrajectoryPoint
    {
        public int Id { get; set; }

        [Required]
        public int ModelRunId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public double AreaSqM { get; set; }

        public double? ThicknessMm { get; set; }

        public string? PolygonGeoJson { get; set; }

        /// <summary>
        /// Cumulative slick footprint (union of all instantaneous polygons up to and including this
        /// timestep), stored as GeoJSON. Used to visualise the total affected area over time.
        /// </summary>
        public string? CumulativePolygonGeoJson { get; set; }

        // Navigation
        public OilSpillModelRun ModelRun { get; set; } = null!;
    }

    public class OilSpillResponseAction
    {
        public int Id { get; set; }

        [Required]
        public int SpillId { get; set; }

        [Required]
        public OilSpillActionType ActionType { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        /// <summary>
        /// GeoJSON geometry (Point, LineString, or Polygon) describing the spatial footprint of
        /// this response measure. Generated from the action type when the action is created.
        /// </summary>
        public string? GeometryGeoJson { get; set; }

        /// <summary>
        /// Effective influence radius in metres (e.g. skimmer recovery radius or dispersant
        /// treatment radius). Null when the measure is purely a line/polygon footprint.
        /// </summary>
        public double? RadiusMeters { get; set; }

        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; } = string.Empty;

        public string? Notes { get; set; }

        // Navigation
        public OilSpillIncident Spill { get; set; } = null!;
    }

    /// <summary>
    /// An IMS/ICS incident-management form (e.g. ICS-201 Briefing, ICS-209 Status Summary,
    /// ICS-214 Activity Log) captured against an oil-spill incident. The structured payload is
    /// stored as JSON in <see cref="JsonData"/> so each form type can evolve independently.
    /// </summary>
    public class IncidentForm
    {
        public int Id { get; set; }

        /// <summary>FK to the owning <see cref="OilSpillIncident"/>.</summary>
        [Required]
        public int IncidentId { get; set; }

        [Required]
        public IncidentFormType FormType { get; set; }

        /// <summary>Structured form payload serialized as JSON.</summary>
        public string JsonData { get; set; } = "{}";

        [Required]
        public IncidentFormStatus Status { get; set; } = IncidentFormStatus.Draft;

        /// <summary>True when this form is automatically included in the SITREP export.</summary>
        public bool IsLinkedToSitrep { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>Path of the most recent exported document for this form, if any.</summary>
        [StringLength(400)]
        public string? ExportFilePath { get; set; }

        // Navigation
        public OilSpillIncident Spill { get; set; } = null!;
    }
}