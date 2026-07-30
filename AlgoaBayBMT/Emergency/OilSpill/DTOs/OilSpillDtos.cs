using AlgoaBayBMT.Emergency.OilSpill.Models;

namespace AlgoaBayBMT.Emergency.OilSpill.DTOs
{
    public class CreateOilSpillRequest
    {
        public int? BunkeringOperationId { get; set; }
        public string SpillName { get; set; } = string.Empty;
        public DateTime SpillStartTime { get; set; }
        public OilSpillProductType ProductType { get; set; }
        public double EstimatedVolume { get; set; }
        public double? ReleaseRate { get; set; }
        public OilSpillSourceType SourceType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>Optional Incident Commander (IMS/ICS) name captured at incident creation.</summary>
        public string? Commander { get; set; }
    }

    public class UpdateOilSpillRequest
    {
        public int Id { get; set; }
        public string? SpillName { get; set; }
        public DateTime? SpillEndTime { get; set; }
        public OilSpillStatus? Status { get; set; }
        public double? EstimatedVolume { get; set; }
        public double? ReleaseRate { get; set; }
    }

    public class OilSpillModelRunRequest
    {
        public int SpillId { get; set; }
        public int? RunId { get; set; }
        public string RunName { get; set; } = "Base Case";
        public DateTime StartTime { get; set; }
        public double DurationHours { get; set; } = 12;
        public int TimeStepMinutes { get; set; } = 30;
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public double CurrentSpeed { get; set; }
        public double CurrentDirection { get; set; }
        public OilSpillTideState TideState { get; set; } = OilSpillTideState.Unknown;
        public string? Notes { get; set; }
    }

    public class CreateOilSpillResponseActionRequest
    {
        public int SpillId { get; set; }
        public OilSpillActionType ActionType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }

        /// <summary>Optional end time after which the measure is no longer active.</summary>
        public DateTime? EndTime { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }

        /// <summary>Optional override for the influence radius (skimmer/dispersant), in metres.</summary>
        public double? RadiusMeters { get; set; }

        /// <summary>Optional override for the deployed length (boom/shoreline protection), in metres.</summary>
        public double? LengthMeters { get; set; }
    }
}
