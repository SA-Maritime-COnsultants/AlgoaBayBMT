using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    /// <summary>
    /// Aggregated key results for a model run, used by both the SITREP and run-summary exports.
    /// </summary>
    public sealed class OilSpillRunSummary
    {
        public OilSpillIncident Spill { get; init; } = null!;
        public OilSpillModelRun Run { get; init; } = null!;
        public IReadOnlyList<OilSpillResponseAction> Actions { get; init; } = Array.Empty<OilSpillResponseAction>();
        public IReadOnlyList<OilSpillChartPoint> ChartPoints { get; init; } = Array.Empty<OilSpillChartPoint>();

        public double MaxAreaSqM { get; init; }
        public double MaxAreaSqKm => MaxAreaSqM / 1_000_000.0;
        public double TotalDriftDistanceKm { get; init; }
        public DateTime? ShorelineImpactTime { get; init; }
        public bool HasShorelineImpact => ShorelineImpactTime is not null;

        public static OilSpillRunSummary Build(
            OilSpillIncident spill,
            OilSpillModelRun run,
            IReadOnlyList<OilSpillTrajectoryPoint> trajectory,
            IReadOnlyList<OilSpillResponseAction> actions)
        {
            var chartPoints = OilSpillVisualizationBuilder.BuildChartPoints(trajectory);

            var maxArea = chartPoints.Count > 0 ? chartPoints.Max(p => p.AreaSqM) : 0.0;
            var driftKm = chartPoints.Count > 0 ? chartPoints[^1].DistanceKm : 0.0;

            return new OilSpillRunSummary
            {
                Spill = spill,
                Run = run,
                Actions = actions,
                ChartPoints = chartPoints,
                MaxAreaSqM = maxArea,
                TotalDriftDistanceKm = driftKm,
                ShorelineImpactTime = run.ShorelineImpactTime
            };
        }
    }

    /// <summary>A generated export file ready to stream to the browser.</summary>
    public sealed record OilSpillExportFile(string FileName, string ContentType, byte[] Content)
    {
        public string ToBase64() => Convert.ToBase64String(Content);
    }
}
