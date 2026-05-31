using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;
using Microsoft.AspNetCore.Components;

namespace AlgoaBayBMT.Components.Pages.Emergency
{
    public partial class OilSpillCharts : ComponentBase
    {
        /// <summary>Ordered trajectory points for the selected model run.</summary>
        [Parameter] public IReadOnlyList<OilSpillTrajectoryPoint> Trajectory { get; set; } = new List<OilSpillTrajectoryPoint>();

        /// <summary>Index of the time-step to highlight (null = none).</summary>
        [Parameter] public int? HighlightIndex { get; set; }

        private List<OilSpillChartPoint> ChartPoints { get; set; } = new();
        private List<OilSpillChartPoint> _highlightDistance = new();
        private List<OilSpillChartPoint> _highlightArea = new();

        protected override void OnParametersSet()
        {
            ChartPoints = OilSpillVisualizationBuilder.BuildChartPoints(Trajectory);

            // Fresh list instances so Syncfusion charts detect the DataSource change.
            var highlight = new List<OilSpillChartPoint>();
            if (HighlightIndex is int idx && idx >= 0 && idx < ChartPoints.Count)
            {
                highlight.Add(ChartPoints[idx]);
            }

            _highlightDistance = highlight;
            _highlightArea = new List<OilSpillChartPoint>(highlight);
        }
    }
}
