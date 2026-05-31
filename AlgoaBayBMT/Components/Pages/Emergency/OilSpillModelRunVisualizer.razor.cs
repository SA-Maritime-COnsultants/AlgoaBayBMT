using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Services;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.DropDowns;

namespace AlgoaBayBMT.Components.Pages.Emergency
{
    public partial class OilSpillModelRunVisualizer : ComponentBase, IDisposable
    {
        [Parameter] public int SpillId { get; set; }

        [SupplyParameterFromQuery(Name = "runId")]
        public int? RunIdQuery { get; set; }

        private bool _loading = true;
        private OilSpillIncident? _spill;

        private List<OilSpillModelRun> _modelRuns = new();
        private int? _selectedRunId;

        private List<OilSpillTrajectoryPoint> _trajectory = new();
        private List<OilSpillResponseAction> _actions = new();
        private List<OilSpillChartPoint> _chartPoints = new();

        private int _currentIndex;
        private bool _showTrajectory = true;
        private bool _showSlick = true;
        private bool _showActions = true;

        // Response-measure layer toggles.
        private bool _showBooms = true;
        private bool _showSkimmers = true;
        private bool _showDispersants = true;
        private bool _showShorelineProtection = true;

        // When true, the slick layer shows the cumulative footprint (union of all steps).
        private bool _showCumulative = true;

        private bool _isPlaying;
        private CancellationTokenSource? _playCts;

        private bool _shorelineImpactDetected;
        private int? _shorelineImpactIndex;

        // Export UI state.
        private bool _exporting;
        private string? _exportMessage;

        // Regenerate state.
        private bool _regenerating;

        private const int FrameDelayMs = 400;

        private int MaxIndex => Math.Max(0, _trajectory.Count - 1);

        private OilSpillModelRun? SelectedRun =>
            _selectedRunId is int id ? _modelRuns.FirstOrDefault(r => r.Id == id) : null;

        private string CurrentTimeLabel =>
            _currentIndex >= 0 && _currentIndex < _chartPoints.Count
                ? _chartPoints[_currentIndex].TimeLabel
                : "—";

        /// <summary>
        /// The simulated timestamp of the current playback step, used to show/hide time-aware
        /// response measures on the map.
        /// </summary>
        private DateTime? CurrentTime =>
            _currentIndex >= 0 && _currentIndex < _trajectory.Count
                ? _trajectory[_currentIndex].Timestamp
                : null;

        protected override async Task OnInitializedAsync()
        {
            _spill = await OilSpillService.GetSpillByIdAsync(SpillId);
            if (_spill is not null)
            {
                _modelRuns = (await OilSpillModelService.GetModelRunsForSpillAsync(SpillId)).ToList();
                _actions = (await OilSpillResponseService.GetActionsForSpillAsync(SpillId)).ToList();

                if (_modelRuns.Count > 0)
                {
                    var initial = RunIdQuery is int rid && _modelRuns.Any(r => r.Id == rid)
                        ? rid
                        : _modelRuns[0].Id;

                    await LoadRunAsync(initial);
                }
            }

            _loading = false;
        }

        private async Task LoadRunAsync(int runId)
        {
            StopPlayback();
            _selectedRunId = runId;
            _trajectory = (await OilSpillModelService.GetTrajectoryAsync(runId))
                .OrderBy(p => p.Timestamp)
                .ToList();
            _chartPoints = OilSpillVisualizationBuilder.BuildChartPoints(_trajectory);
            _currentIndex = 0;

            // Restore any previously recorded shoreline impact for this run (flag only - the
            // animation plays through the impact rather than stopping at it).
            var run = _modelRuns.FirstOrDefault(r => r.Id == runId);
            if (run?.ShorelineImpactIndex is int impactIndex
                && impactIndex >= 0 && impactIndex <= MaxIndex)
            {
                _shorelineImpactDetected = true;
                _shorelineImpactIndex = impactIndex;
            }
            else
            {
                _shorelineImpactDetected = false;
                _shorelineImpactIndex = null;

                // Scan the whole trajectory up-front so an impact is surfaced immediately,
                // even before the user presses Play.
                if (FindShorelineImpact(MaxIndex) is int detected)
                {
                    await ApplyShorelineImpactAsync(detected);
                }
            }
        }

        /// <summary>
        /// Returns the index of the first trajectory step whose slick polygon intersects the
        /// coastline land geometry, or null if none of the steps up to and including
        /// <paramref name="upToIndex"/> reach the shoreline.
        /// </summary>
        private int? FindShorelineImpact(int upToIndex)
        {
            if (!CoastlineService.HasCoastline)
            {
                return null;
            }

            for (var i = 0; i <= upToIndex && i < _trajectory.Count; i++)
            {
                if (CoastlineService.IntersectsLand(_trajectory[i].PolygonGeoJson))
                {
                    return i;
                }
            }

            return null;
        }

        private async Task ApplyShorelineImpactAsync(int impactIndex)
        {
            // Record the impact and surface the alert, but keep the animation running so the
            // user can watch the slick continue to spread along/onto the shoreline.
            _shorelineImpactDetected = true;
            _shorelineImpactIndex = impactIndex;

            // Persist the impact on the run and update the in-memory copy.
            if (_selectedRunId is int runId)
            {
                var impactTime = _trajectory[impactIndex].Timestamp;
                await OilSpillModelService.RecordShorelineImpactAsync(runId, impactIndex, impactTime);

                var run = _modelRuns.FirstOrDefault(r => r.Id == runId);
                if (run is not null)
                {
                    run.ShorelineImpactIndex = impactIndex;
                    run.ShorelineImpactTime = impactTime;
                }
            }
        }

        private async Task OnRunChanged(ChangeEventArgs<int?, OilSpillModelRun> args)
        {
            if (args.Value is int runId && runId != _selectedRunId)
            {
                await LoadRunAsync(runId);
            }
        }

        private void OnTrajectoryToggled(bool value)
        {
            _showTrajectory = value;
            StateHasChanged();
        }

        private void OnSlickToggled(bool value)
        {
            _showSlick = value;
            StateHasChanged();
        }

        private void OnActionsToggled(bool value)
        {
            _showActions = value;
            StateHasChanged();
        }

        private void OnBoomsToggled(bool value)
        {
            _showBooms = value;
            StateHasChanged();
        }

        private void OnSkimmersToggled(bool value)
        {
            _showSkimmers = value;
            StateHasChanged();
        }

        private void OnDispersantsToggled(bool value)
        {
            _showDispersants = value;
            StateHasChanged();
        }

        private void OnShorelineProtectionToggled(bool value)
        {
            _showShorelineProtection = value;
            StateHasChanged();
        }

        private void OnSliderChanged(int index)
        {
            var clamped = Math.Clamp(index, 0, MaxIndex);

            // Ignore programmatic echoes (e.g. the slider reflecting an animation frame).
            if (clamped == _currentIndex)
            {
                return;
            }

            // A genuine user scrub should take over from any running animation.
            StopPlayback();
            _currentIndex = clamped;
            StateHasChanged();
        }

        private async Task TogglePlay()
        {
            if (_isPlaying)
            {
                StopPlayback();
            }
            else
            {
                await StartPlaybackAsync();
            }
        }

        private async Task StartPlaybackAsync()
        {
            if (_isPlaying || _trajectory.Count < 2)
            {
                return;
            }

            // Restart from the beginning when the playhead is already at the end.
            if (_currentIndex >= MaxIndex)
            {
                _currentIndex = 0;
            }

            // Surface an impact for the starting frame (flag only - playback still proceeds).
            if (FindShorelineImpact(_currentIndex) is int startImpact)
            {
                await ApplyShorelineImpactAsync(startImpact);
            }

            _isPlaying = true;
            _playCts = new CancellationTokenSource();
            var token = _playCts.Token;

            try
            {
                while (!token.IsCancellationRequested && _currentIndex < MaxIndex)
                {
                    await Task.Delay(FrameDelayMs, token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    _currentIndex++;

                    // Flag the shoreline impact when first reached but keep advancing the
                    // animation - it now runs continuously until paused or the end is reached.
                    if (!_shorelineImpactDetected
                        && FindShorelineImpact(_currentIndex) is int impactIndex)
                    {
                        await ApplyShorelineImpactAsync(impactIndex);
                    }

                    StateHasChanged();
                }
            }
            catch (TaskCanceledException)
            {
                // Playback was stopped - expected.
            }
            finally
            {
                _isPlaying = false;
                StateHasChanged();
            }
        }

        /// <summary>Resets the playhead to the start and begins playback again.</summary>
        private async Task ReplayAsync()
        {
            StopPlayback();
            _currentIndex = 0;
            StateHasChanged();
            await StartPlaybackAsync();
        }

        /// <summary>
        /// Re-runs the currently selected model using its existing parameters so that the updated
        /// coastline clipping, cumulative footprint, and coastal-spread geometry is regenerated.
        /// </summary>
        private async Task RegenerateRunAsync()
        {
            if (SelectedRun is not { } run || _spill is null)
            {
                return;
            }

            StopPlayback();
            _regenerating = true;
            _exportMessage = "Regenerating model run…";
            StateHasChanged();

            try
            {
                var request = new AlgoaBayBMT.Emergency.OilSpill.DTOs.OilSpillModelRunRequest
                {
                    SpillId = run.SpillId,
                    RunId = run.Id,
                    RunName = run.RunName,
                    StartTime = run.StartTime,
                    DurationHours = run.DurationHours,
                    TimeStepMinutes = run.TimeStepMinutes,
                    WindSpeed = run.WindSpeed,
                    WindDirection = run.WindDirection,
                    CurrentSpeed = run.CurrentSpeed,
                    CurrentDirection = run.CurrentDirection,
                    TideState = run.TideState,
                    Notes = run.Notes
                };

                await OilSpillModelService.RunModelAsync(request);

                // Reload the fresh trajectory and reset playback.
                _shorelineImpactDetected = false;
                _shorelineImpactIndex = null;
                await LoadRunAsync(run.Id);
                _exportMessage = "Run regenerated successfully. Trajectory updated.";
            }
            catch (Exception ex)
            {
                _exportMessage = $"Regeneration failed: {ex.Message}";
            }
            finally
            {
                _regenerating = false;
                StateHasChanged();
            }
        }

        private void OnCumulativeToggled(bool value)
        {
            _showCumulative = value;
            StateHasChanged();
        }

        private void GoToComparison()
            => NavigationManager.NavigateTo($"/emergency/oilspill/{SpillId}/compare");

        private async Task ExportMapSnapshotAsync()
        {
            if (_trajectory.Count == 0)
            {
                return;
            }

            StopPlayback();
            _exporting = true;
            _exportMessage = "Capturing map snapshot…";
            StateHasChanged();

            try
            {
                var fileName = $"OilSpillMap_{SpillId}_{_selectedRunId ?? 0}.png";
                var ok = await JS.InvokeAsync<bool>(
                    "oilSpillExport.captureElementToPng", "oilspill-map-capture", fileName);
                _exportMessage = ok
                    ? "Map snapshot downloaded."
                    : "Unable to capture the map snapshot.";
            }
            catch (Exception ex)
            {
                _exportMessage = $"Snapshot export failed: {ex.Message}";
            }
            finally
            {
                _exporting = false;
                StateHasChanged();
            }
        }

        private async Task ExportSitrepAsync()
        {
            if (SelectedRun is not { } run || _spill is null)
            {
                return;
            }

            StopPlayback();
            _exporting = true;
            _exportMessage = "Generating SITREP…";
            StateHasChanged();

            try
            {
                // Try to embed the current map view in the SITREP.
                byte[]? snapshot = null;
                try
                {
                    var base64 = await JS.InvokeAsync<string?>(
                        "oilSpillExport.captureElementToBase64", "oilspill-map-capture");
                    if (!string.IsNullOrEmpty(base64))
                    {
                        snapshot = Convert.FromBase64String(base64);
                    }
                }
                catch
                {
                    // Snapshot is optional - continue without it.
                }

                var summary = OilSpillRunSummary.Build(_spill, run, _trajectory, _actions);
                var file = OilSpillExportService.GenerateSitrepPdf(summary, snapshot);
                await DownloadAsync(file);
                _exportMessage = "SITREP PDF downloaded.";
            }
            catch (Exception ex)
            {
                _exportMessage = $"SITREP export failed: {ex.Message}";
            }
            finally
            {
                _exporting = false;
                StateHasChanged();
            }
        }

        private async Task ExportRunSummaryAsync()
        {
            if (SelectedRun is not { } run || _spill is null)
            {
                return;
            }

            StopPlayback();
            _exporting = true;
            _exportMessage = "Generating run summary…";
            StateHasChanged();

            try
            {
                var summary = OilSpillRunSummary.Build(_spill, run, _trajectory, _actions);
                var file = OilSpillExportService.GenerateRunSummaryPdf(summary);
                await DownloadAsync(file);
                _exportMessage = "Run summary PDF downloaded.";
            }
            catch (Exception ex)
            {
                _exportMessage = $"Run summary export failed: {ex.Message}";
            }
            finally
            {
                _exporting = false;
                StateHasChanged();
            }
        }

        private async Task DownloadAsync(OilSpillExportFile file)
        {
            await JS.InvokeVoidAsync(
                "oilSpillExport.downloadFile", file.FileName, file.ToBase64(), file.ContentType);
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

        private void BackToDetail()
            => NavigationManager.NavigateTo($"/emergency/oilspill/{SpillId}");

        public void Dispose() => StopPlayback();
    }
}
