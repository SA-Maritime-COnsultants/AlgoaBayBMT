using AlgoaBayBMT.Emergency.OilSpill.Models;

namespace AlgoaBayBMT.Emergency.OilSpill.DTOs
{
    /// <summary>
    /// Strongly-typed payload for an ICS-209 Incident Status Summary. Serialized to
    /// <see cref="IncidentForm.JsonData"/>. Auto-populated from the incident and latest model run.
    /// </summary>
    public class Ics209Payload
    {
        public string IncidentName { get; set; } = string.Empty;
        public string Commander { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double EstimatedVolume { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime ReportTime { get; set; } = DateTime.UtcNow;

        public string SituationSummary { get; set; } = string.Empty;
        public double MaxSlickAreaSqKm { get; set; }
        public double TotalDriftDistanceKm { get; set; }
        public bool HasShorelineImpact { get; set; }
        public DateTime? ShorelineImpactTime { get; set; }
        public int ResponseMeasureCount { get; set; }
        public string PlannedActions { get; set; } = string.Empty;
    }

    /// <summary>Strongly-typed payload for an ICS-201 Incident Briefing.</summary>
    public class Ics201Payload
    {
        public string IncidentName { get; set; } = string.Empty;
        public string Commander { get; set; } = string.Empty;
        public DateTime IncidentStartTime { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public double EstimatedVolume { get; set; }
        public string CurrentSituation { get; set; } = string.Empty;
        public string InitialObjectives { get; set; } = string.Empty;
        public string SafetyMessage { get; set; } = string.Empty;
    }

    /// <summary>Strongly-typed payload for an ICS-214 Activity Log.</summary>
    public class Ics214Payload
    {
        public string IncidentName { get; set; } = string.Empty;
        public List<Ics214Entry> Entries { get; set; } = new();
    }

    public class Ics214Entry
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Activity { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
    }
}
