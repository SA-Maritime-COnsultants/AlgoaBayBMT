using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Services;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;

namespace AlgoaBayBMT.Components.Pages.Emergency
{
    public partial class OilSpillScenarioComparison : ComponentBase, IDisposable
    {
        [Parameter] public int SpillId { get; set; }

        private bool _loading = true;
        private OilSpillIncident? _spill;

        private List<OilSpillModelRun> _modelRuns = new();
        private List<OilSpillResponseAction> _actions = new();

        private int? _baseRunId;
        private int? _altRunId;
        private OilSpillModelRun? _baseRun;
        private OilSpillModelRun? _altRun;

        private List<OilSpillTrajectoryPoint> _baseTrajectory = new();
        private List<OilSpillTrajectoryPoint> _altTrajectory = new();

        private OilSpillRunSummary? _baseSummary;
        private OilSpillRunSummary? _altSummary;
        private RunMetrics _baseMetrics;
        private RunMetrics _altMetrics;

        private int _sharedIndex;
        private bool _showCumulative;

        private bool _isPlaying;
        private CancellationTokenSource? _playCts;

        private const int FrameDelayMs = 400;

        // The shared slider spans the longer of the two trajectories.
        private int SharedMaxIndex =>
            Math.Max(0, Math.Max(_baseTrajectory.Count, _altTrajectory.Count) - 1);

        private DateTime? BaseCurrentTime => TimeAt(_baseTrajectory);
        private DateTime? AltCurrentTime => TimeAt(_altTrajectory);

        protected override async Task OnInitializedAsync()
        {
            _spill = await OilSpillService.GetSpillByIdAsync(SpillId);
            if (_spill is not null)
            {
                _modelRuns = (await OilSpillModelService.GetModelRunsForSpillAsync(SpillId)).ToList();
                _actions = (await OilSpillResponseService.GetActionsForSpillAsync(SpillId)).ToList();

                if (_modelRuns.Count >= 2)
                {
                    _baseRunId = _modelRuns[0].Id;
                    _altRunId = _modelRuns[1].Id;
                    await LoadBaseAsync(_baseRunId.Value);
                    await LoadAltAsync(_altRunId.Value);
                }
            }

            _loading = false;
        }

        private async Task LoadBaseAsync(int runId)
        {
            _baseRun = _modelRuns.FirstOrDefault(r => r.Id == runId);
            _baseTrajectory = (await OilSpillModelService.GetTrajectoryAsync(runId)).ToList();
            _baseSummary = _baseRun is not null && _spill is not null
                ? OilSpillRunSummary.Build(_spill, _baseRun, _baseTrajectory, _actions)
                : null;
            _baseMetrics = RunMetrics.From(_baseSummary);
            ClampSharedIndex();
        }

        private async Task LoadAltAsync(int runId)
        {
            _altRun = _modelRuns.FirstOrDefault(r => r.Id == runId);
            _altTrajectory = (await OilSpillModelService.GetTrajectoryAsync(runId)).ToList();
            _altSummary = _altRun is not null && _spill is not null
                ? OilSpillRunSummary.Build(_spill, _altRun, _altTrajectory, _actions)
                : null;
            _altMetrics = RunMetrics.From(_altSummary);
            ClampSharedIndex();
        }

        private async Task OnBaseRunChanged(ChangeEventArgs<int?, OilSpillModelRun> args)
        {
            StopPlayback();
            if (args.Value is int runId && runId != _baseRunId)
            {
                _baseRunId = runId;
                await LoadBaseAsync(runId);
                StateHasChanged();
            }
        }

        private async Task OnAltRunChanged(ChangeEventArgs<int?, OilSpillModelRun> args)
        {
            StopPlayback();
            if (args.Value is int runId && runId != _altRunId)
            {
                _altRunId = runId;
                await LoadAltAsync(runId);
                StateHasChanged();
            }
        }

        private void OnCumulativeToggled(bool value)
        {
            _showCumulative = value;
            StateHasChanged();
        }

        private void OnSliderChanged(int index)
        {
            StopPlayback();
            _sharedIndex = Math.Clamp(index, 0, SharedMaxIndex);
            StateHasChanged();
        }

        private async Task TogglePlay()
        {
            if (_isPlaying)
            {
                StopPlayback();
                StateHasChanged();
            }
            else
            {
                await StartPlaybackAsync();
            }
        }

        private async Task StartPlaybackAsync()
        {
            if (_isPlaying || SharedMaxIndex < 1)
            {
                return;
            }

            if (_sharedIndex >= SharedMaxIndex)
            {
                _sharedIndex = 0;
            }

            _isPlaying = true;
            _playCts = new CancellationTokenSource();
            var token = _playCts.Token;

            try
            {
                while (!token.IsCancellationRequested && _sharedIndex < SharedMaxIndex)
                {
                    await Task.Delay(FrameDelayMs, token);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    _sharedIndex++;
                    StateHasChanged();
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when playback is paused.
            }
            finally
            {
                _isPlaying = false;
                StateHasChanged();
            }
        }

        private void StopPlayback()
        {
            if (_playCts is not null)
            {
                _playCts.Cancel();
                _playCts.Dispose();
                _playCts = null;
            }

            _isPlaying = false;
        }

        private int? ClampIndex(IReadOnlyList<OilSpillTrajectoryPoint> trajectory)
        {
            if (trajectory.Count == 0)
            {
                return null;
            }

            return Math.Min(_sharedIndex, trajectory.Count - 1);
        }

        private DateTime? TimeAt(IReadOnlyList<OilSpillTrajectoryPoint> trajectory)
        {
            if (trajectory.Count == 0)
            {
                return null;
            }

            var idx = Math.Min(_sharedIndex, trajectory.Count - 1);
            return trajectory[idx].Timestamp;
        }

        private void ClampSharedIndex()
            => _sharedIndex = Math.Clamp(_sharedIndex, 0, SharedMaxIndex);

        private static string FormatDiff(double value, string format)
        {
            var sign = value > 0 ? "+" : string.Empty;
            return $"{sign}{value.ToString(format)}";
        }

        private static string FormatImpact(RunMetrics metrics)
            => metrics.ShorelineImpactTime is { } time
                ? time.ToString("dd MMM HH:mm")
                : "No impact";

        private string ImpactComparison()
        {
            var baseImpact = _baseMetrics.ShorelineImpactTime;
            var altImpact = _altMetrics.ShorelineImpactTime;

            if (baseImpact is null && altImpact is null)
            {
                return "Both avoid shore";
            }

            if (baseImpact is null)
            {
                return "Alternative reaches shore";
            }

            if (altImpact is null)
            {
                return "Alternative avoids shore";
            }

            var delta = altImpact.Value - baseImpact.Value;
            var minutes = (int)Math.Round(delta.TotalMinutes);
            if (minutes == 0)
            {
                return "Same impact time";
            }

            return minutes > 0
                ? $"Alternative {minutes} min later"
                : $"Alternative {Math.Abs(minutes)} min sooner";
        }

        private void BackToVisualizer()
            => NavigationManager.NavigateTo($"/emergency/oilspill/{SpillId}/visualize");

        public void Dispose() => StopPlayback();

        private readonly record struct RunMetrics(
            double MaxAreaSqKm, double TotalDriftDistanceKm, DateTime? ShorelineImpactTime)
        {
            public static RunMetrics From(OilSpillRunSummary? summary)
                => summary is null
                    ? new RunMetrics(0, 0, null)
                    : new RunMetrics(
                        summary.MaxAreaSqKm,
                        summary.TotalDriftDistanceKm,
                        summary.ShorelineImpactTime);
        }
    }
}
