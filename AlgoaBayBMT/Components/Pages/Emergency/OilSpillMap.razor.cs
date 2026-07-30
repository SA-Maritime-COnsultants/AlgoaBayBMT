using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Maps;

namespace AlgoaBayBMT.Components.Pages.Emergency
{
    public partial class OilSpillMap : ComponentBase
    {
        /// <summary>Ordered trajectory points for the selected model run.</summary>
        [Parameter] public IReadOnlyList<OilSpillTrajectoryPoint> Trajectory { get; set; } = new List<OilSpillTrajectoryPoint>();

        /// <summary>Response actions with coordinates to plot.</summary>
        [Parameter] public IReadOnlyList<OilSpillResponseAction> Actions { get; set; } = new List<OilSpillResponseAction>();

        /// <summary>Vessel name shown against the ship icon at the spill origin.</summary>
        [Parameter] public string? VesselName { get; set; }

        /// <summary>Index of the time-step to highlight (null = none).</summary>
        [Parameter] public int? HighlightIndex { get; set; }

        [Parameter] public bool ShowTrajectory { get; set; } = true;
        [Parameter] public bool ShowSlick { get; set; } = true;
        [Parameter] public bool ShowActions { get; set; } = true;

        /// <summary>
        /// When true the slick layer renders the cumulative footprint (union of every step) instead
        /// of the instantaneous slick polygon for the current timestep.
        /// </summary>
        [Parameter] public bool ShowCumulative { get; set; } = true;

        /// <summary>When true the slick polygon is rendered in alarm red (shoreline impact).</summary>
        [Parameter] public bool ShorelineImpact { get; set; }

        /// <summary>Trajectory index where the slick reaches the shoreline (null = none).</summary>
        [Parameter] public int? ImpactIndex { get; set; }

        /// <summary>
        /// GeoJSON line geometry of the impacted coastline stretch. Rendered as a thick red line
        /// hugging the shore so the potential shoreline impact zone is explicit.
        /// </summary>
        [Parameter] public string? ImpactZoneGeoJson { get; set; }

        /// <summary>
        /// Optional second-scenario trajectory (e.g. the WITH-measures prediction overlaid on the
        /// WITHOUT-measures one). Its cumulative footprint is drawn as a green comparison outline.
        /// </summary>
        [Parameter] public IReadOnlyList<OilSpillTrajectoryPoint>? ComparisonTrajectory { get; set; }

        /// <summary>When true the comparison scenario footprint overlay is rendered.</summary>
        [Parameter] public bool ShowComparison { get; set; }

        /// <summary>Spilled product type used to colour-code the slick polygon.</summary>
        [Parameter] public OilSpillProductType ProductType { get; set; } = OilSpillProductType.Unknown;

        /// <summary>Current playback time used to show/hide time-aware response measures.</summary>
        [Parameter] public DateTime? CurrentTime { get; set; }

        [Parameter] public bool ShowBooms { get; set; } = true;
        [Parameter] public bool ShowSkimmers { get; set; } = true;
        [Parameter] public bool ShowDispersants { get; set; } = true;
        [Parameter] public bool ShowShorelineProtection { get; set; } = true;

        /// <summary>When true a small "T+Xh" time label is drawn at each plume centroid.</summary>
        [Parameter] public bool ShowTimeLabels { get; set; } = true;

        /// <summary>
        /// When true the map is in "placement" mode: a map click reports its geographic coordinate
        /// through <see cref="OnMapClick"/> so a response measure can be dropped where the user
        /// clicks instead of typing latitude/longitude by hand.
        /// </summary>
        [Parameter] public bool PlacementMode { get; set; }

        /// <summary>
        /// Raised with the (latitude, longitude) of a map click while <see cref="PlacementMode"/> is
        /// active. The visualiser uses this to position a pending response measure.
        /// </summary>
        [Parameter] public EventCallback<(double Latitude, double Longitude)> OnMapClick { get; set; }

        /// <summary>
        /// Location of a not-yet-saved response measure (e.g. the point the user just clicked). When
        /// set a distinct crosshair pin is drawn so the placement is visible before saving.
        /// </summary>
        [Parameter] public (double Latitude, double Longitude)? PendingMeasure { get; set; }

        private List<OilSpillMapPoint> _pendingMarker = new();

        private SfMaps? _mapsRef;
        private int? _renderedHighlightIndex;
        private (double Latitude, double Longitude)? _renderedPendingMeasure;
        private bool _refreshQueued;

        // Backing fields bound by the razor markup.
        private double _centerLat;
        private double _centerLon;
        private double _zoomFactor = 11;

        private List<OilSpillMapPoint> MapPoints { get; set; } = new();
        private List<OilSpillMapPoint> _originMarker = new();
        private List<OilSpillMapPoint> _stepMarkers = new();
        private List<OilSpillMapPoint> _highlightMarker = new();
        private List<OilSpillMapPoint> _impactMarker = new();
        private List<OilSpillActionMarker> _actionMarkers = new();

        private List<Coordinate> _polygonPoints = new();
        private List<List<Coordinate>> _slickRings = new();
        private List<List<Coordinate>> _comparisonRings = new();
        private List<ResponseLine> _impactZoneLines = new();
        private double[] _polylineLat = Array.Empty<double>();
        private double[] _polylineLon = Array.Empty<double>();
        private List<OilSpillMapPoint> _polylinePoints = new();

        // Response measures parsed into renderable geometry, grouped for distinct styling.
        private List<OilSpillResponseFeature> _responseFeatures = new();
        private List<ResponseLine> _boomLines = new();
        private List<ResponseLine> _shorelineLines = new();
        private List<ResponseLine> _reconLines = new();
        private List<List<Coordinate>> _dispersantPolygons = new();
        private List<OilSpillMapPoint> _skimmerMarkers = new();
        private List<OilSpillMapPoint> _reconMarkers = new();

        // Plume time labels ("T+Xh") rendered at polygon centroids.
        private List<OilSpillTimeLabel> _timeLabels = new();

        /// <summary>A plume time label positioned at a polygon centroid.</summary>
        private sealed class OilSpillTimeLabel
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        /// <summary>A response line geometry ready for a Syncfusion navigation line.</summary>
        private sealed class ResponseLine
        {
            public double[] Latitudes { get; set; } = Array.Empty<double>();
            public double[] Longitudes { get; set; } = Array.Empty<double>();
        }

        protected override void OnParametersSet()
        {
            MapPoints = OilSpillVisualizationBuilder.BuildMapPoints(Trajectory);
            _actionMarkers = OilSpillVisualizationBuilder.BuildActionMarkers(Actions);
            _responseFeatures = OilSpillVisualizationBuilder.BuildResponseFeatures(Actions);

            BuildMarkers();
            BuildPolyline();
            BuildPolygon();
            BuildComparisonPolygon();
            BuildImpactZone();
            BuildResponseGeometry();
            BuildTimeLabels();
            BuildPendingMarker();
            ComputeCenter();

            // Queue a Maps redraw whenever the highlighted timestep changes so the
            // moving marker and growing slick are reflected on the rendered SVG.
            if (HighlightIndex != _renderedHighlightIndex)
            {
                _refreshQueued = true;
            }

            // Also redraw when the pending measure pin moves (each map click while placing) so the
            // crosshair follows the click on the rendered SVG.
            if (!NullableCoordEquals(PendingMeasure, _renderedPendingMeasure))
            {
                _refreshQueued = true;
            }
        }

        private static bool NullableCoordEquals(
            (double Latitude, double Longitude)? a, (double Latitude, double Longitude)? b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Value.Latitude.Equals(b.Value.Latitude)
                && a.Value.Longitude.Equals(b.Value.Longitude);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (_refreshQueued && _mapsRef is not null)
            {
                _refreshQueued = false;
                _renderedHighlightIndex = HighlightIndex;
                _renderedPendingMeasure = PendingMeasure;

                try
                {
                    _mapsRef.Refresh();
                }
                catch (InvalidOperationException)
                {
                    // Circuit/JS interop not available (e.g. during teardown) - ignore.
                }
            }
        }

        /// <summary>
        /// Builds the single pending-measure marker (the point the user just clicked while placing a
        /// response measure) so it can be drawn as a crosshair pin before the measure is saved.
        /// </summary>
        private void BuildPendingMarker()
        {
            var pending = new List<OilSpillMapPoint>();
            if (PendingMeasure is { } p && IsFinite(p.Latitude) && IsFinite(p.Longitude))
            {
                pending.Add(new OilSpillMapPoint
                {
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Label = "New measure",
                    Tooltip = "Pending response measure — click Save to apply"
                });
            }

            _pendingMarker = pending;
        }

        /// <summary>
        /// Handles a click on the map. In placement mode the clicked geographic coordinate is passed
        /// up to the visualiser so the response-measure form can be positioned by pointing at the
        /// chart instead of typing coordinates.
        /// </summary>
        private async Task HandleMapClick(MouseEventArgs args)
        {
            if (!PlacementMode || !OnMapClick.HasDelegate)
            {
                return;
            }

            if (!IsFinite(args.Latitude) || !IsFinite(args.Longitude))
            {
                return;
            }

            await OnMapClick.InvokeAsync((args.Latitude, args.Longitude));
        }

        private void BuildMarkers()
        {
            // Assign fresh list instances so Syncfusion detects the DataSource change
            // (it compares by reference, so in-place mutation would not re-render).
            var origin = new List<OilSpillMapPoint>();
            var steps = new List<OilSpillMapPoint>();
            var highlight = new List<OilSpillMapPoint>();
            var impact = new List<OilSpillMapPoint>();

            // Drift stops at shoreline impact, so the plotted track also stops there: any points
            // after ImpactIndex are pinned at the landfall coordinate (alongshore-spreading steps)
            // and would just stack markers on top of the impact zone.
            var lastIndex = MapPoints.Count - 1;
            if (ImpactIndex is int cutoff && cutoff >= 0 && cutoff < MapPoints.Count)
            {
                lastIndex = cutoff;
            }

            if (MapPoints.Count > 0)
            {
                // Origin marker carries the vessel name so the map template can label the ship
                // icon at the spill start position.
                var start = MapPoints[0];
                origin.Add(new OilSpillMapPoint
                {
                    Index = start.Index,
                    Latitude = start.Latitude,
                    Longitude = start.Longitude,
                    Label = string.IsNullOrWhiteSpace(VesselName) ? "Spill origin" : VesselName!,
                    Tooltip = string.IsNullOrWhiteSpace(VesselName)
                        ? start.Tooltip
                        : $"{VesselName} — spill origin",
                    AreaSqM = start.AreaSqM
                });

                // All points except origin up to (and including) the cut-off are step markers.
                for (var i = 1; i <= lastIndex; i++)
                {
                    steps.Add(MapPoints[i]);
                }

                // Clamp the highlight to the visible portion of the track.
                if (HighlightIndex is int idx && idx >= 0 && idx < MapPoints.Count)
                {
                    var clamped = Math.Min(idx, lastIndex);
                    highlight.Add(MapPoints[clamped]);
                }

                if (ImpactIndex is int markIdx && markIdx >= 0 && markIdx < MapPoints.Count)
                {
                    impact.Add(MapPoints[markIdx]);
                }
            }

            _originMarker = origin;
            _stepMarkers = steps;
            _highlightMarker = highlight;
            _impactMarker = impact;
        }

        private void BuildPolyline()
        {
            // The drift track ends at the shoreline impact (drift stops there); post-impact
            // points repeat the landfall coordinate so they add nothing to the line.
            var visible = MapPoints;
            if (ImpactIndex is int cutoff && cutoff >= 0 && cutoff < MapPoints.Count)
            {
                visible = MapPoints.Take(cutoff + 1).ToList();
            }

            _polylinePoints = visible;
            _polylineLat = visible.Select(p => p.Latitude).ToArray();
            _polylineLon = visible.Select(p => p.Longitude).ToArray();
        }

        private void BuildPolygon()
        {
            // Resolve the trajectory point representing the current timestep (or the last
            // point when no specific step is highlighted) so the slick reflects playback.
            int sourceIndex;
            if (HighlightIndex is int idx && idx >= 0 && idx < Trajectory.Count)
            {
                sourceIndex = idx;
            }
            else if (Trajectory.Count > 0)
            {
                sourceIndex = Trajectory.Count - 1;
            }
            else
            {
                _slickRings = new List<List<Coordinate>>();
                _polygonPoints = new List<Coordinate>();
                return;
            }

            var source = Trajectory[sourceIndex];

            // The full plume is the cumulative footprint from origin up to the current step. When the
            // cumulative toggle is on (default) prefer the stored cumulative polygon; otherwise show
            // just the instantaneous slick for this step. Fall back gracefully when data is missing.
            var geoJson = ShowCumulative
                ? source.CumulativePolygonGeoJson ?? source.PolygonGeoJson
                : source.PolygonGeoJson ?? source.CumulativePolygonGeoJson;

            var rings = !string.IsNullOrWhiteSpace(geoJson)
                ? OilSpillGeometry.ParsePolygonRings(geoJson)
                : new List<List<(double Latitude, double Longitude)>>();

            // Fall back to a growing circle when no polygon geometry is available at all.
            if (rings.Count == 0)
            {
                var circle = OilSpillGeometry.GenerateCirclePolygon(
                    source.Latitude,
                    source.Longitude,
                    OilSpillGeometry.RadiusFromAreaMeters(source.AreaSqM));
                if (circle.Count >= 3)
                {
                    rings.Add(circle);
                }
            }

            _slickRings = rings
                .Select(ring => ring
                    .Where(p => IsFinite(p.Latitude) && IsFinite(p.Longitude)
                                && Math.Abs(p.Latitude) <= 90 && Math.Abs(p.Longitude) <= 180)
                    .Select(p => new Coordinate { Latitude = p.Latitude, Longitude = p.Longitude })
                    .ToList())
                .Where(ring => ring.Count >= 3 && IsRingNearOrigin(ring, source))
                .ToList();

            // Keep the legacy single-ring field populated (largest ring) for centring logic.
            _polygonPoints = _slickRings.Count > 0
                ? _slickRings.OrderByDescending(r => r.Count).First()
                : new List<Coordinate>();
        }

        /// <summary>
        /// Builds the comparison-scenario footprint rings (e.g. the WITH-measures prediction drawn
        /// over the WITHOUT-measures slick). Uses the cumulative footprint at the matching playback
        /// step so both scenarios are compared at the same simulated time.
        /// </summary>
        private void BuildComparisonPolygon()
        {
            _comparisonRings = new List<List<Coordinate>>();

            if (!ShowComparison || ComparisonTrajectory is null || ComparisonTrajectory.Count == 0)
            {
                return;
            }

            var index = HighlightIndex is int idx && idx >= 0
                ? Math.Min(idx, ComparisonTrajectory.Count - 1)
                : ComparisonTrajectory.Count - 1;

            var source = ComparisonTrajectory[index];
            var geoJson = source.CumulativePolygonGeoJson ?? source.PolygonGeoJson;
            if (string.IsNullOrWhiteSpace(geoJson))
            {
                return;
            }

            _comparisonRings = OilSpillGeometry.ParsePolygonRings(geoJson)
                .Select(ring => ring
                    .Where(p => IsFinite(p.Latitude) && IsFinite(p.Longitude)
                                && Math.Abs(p.Latitude) <= 90 && Math.Abs(p.Longitude) <= 180)
                    .Select(p => new Coordinate { Latitude = p.Latitude, Longitude = p.Longitude })
                    .ToList())
                .Where(ring => ring.Count >= 3 && IsRingNearOrigin(ring, source))
                .ToList();
        }

        /// <summary>
        /// Parses the impacted-coastline GeoJSON into renderable polylines so the potential
        /// shoreline impact zone is drawn as a red band along the shore.
        /// </summary>
        private void BuildImpactZone()
        {
            var lines = new List<ResponseLine>();

            foreach (var line in OilSpillGeometry.ParseLineStrings(ImpactZoneGeoJson))
            {
                var clean = line
                    .Where(p => IsFinite(p.Latitude) && IsFinite(p.Longitude)
                                && Math.Abs(p.Latitude) <= 90 && Math.Abs(p.Longitude) <= 180)
                    .ToList();

                if (clean.Count >= 2)
                {
                    lines.Add(ToResponseLine(clean));
                }
            }

            _impactZoneLines = lines;
        }

        /// <summary>
        /// Builds the plume time labels ("T+Xh") positioned at each trajectory polygon centroid.
        /// One label per timestep up to (and including) the current playback index, so labels
        /// accumulate as the animation advances. Skipped entirely when ShowTimeLabels is false.
        /// </summary>
        private void BuildTimeLabels()
        {
            var labels = new List<OilSpillTimeLabel>();

            if (!ShowTimeLabels || Trajectory.Count == 0)
            {
                _timeLabels = labels;
                return;
            }

            var lastIndex = HighlightIndex is int idx && idx >= 0 && idx < Trajectory.Count
                ? idx
                : Trajectory.Count - 1;

            var origin = Trajectory[0].Timestamp;

            for (var i = 0; i <= lastIndex && i < Trajectory.Count; i++)
            {
                var point = Trajectory[i];

                // Prefer the centroid of the instantaneous slick polygon; fall back to the stored
                // trajectory coordinate when no usable polygon geometry exists for this step.
                var centroid = OilSpillGeometry.PolygonCentroid(point.PolygonGeoJson)
                               ?? (point.Latitude, point.Longitude);

                if (!IsFinite(centroid.Latitude) || !IsFinite(centroid.Longitude))
                {
                    continue;
                }

                // Skip labels that would sit on top of the previous one (e.g. the alongshore-
                // spreading steps after landfall, which are pinned near the same centroid).
                if (labels.Count > 0)
                {
                    var prev = labels[^1];
                    if (Math.Abs(prev.Latitude - centroid.Latitude) < 0.002
                        && Math.Abs(prev.Longitude - centroid.Longitude) < 0.002)
                    {
                        continue;
                    }
                }

                var hours = (point.Timestamp - origin).TotalHours;
                var text = hours <= 0.01
                    ? "T+0h"
                    : $"T+{hours:0.#}h";

                labels.Add(new OilSpillTimeLabel
                {
                    Latitude = centroid.Latitude,
                    Longitude = centroid.Longitude,
                    Text = text
                });
            }

            _timeLabels = labels;
        }

        /// <summary>
        /// Guards against malformed slick rings: every vertex must sit within a sane distance
        /// (~2 degrees, roughly 200 km) of the trajectory point it belongs to. A stray vertex from
        /// bad GeoJSON would otherwise draw the plume in the wrong place / across the continent.
        /// </summary>
        private static bool IsRingNearOrigin(List<Coordinate> ring, OilSpillTrajectoryPoint source)
        {
            const double MaxDegrees = 2.0;
            foreach (var c in ring)
            {
                if (Math.Abs(c.Latitude - source.Latitude) > MaxDegrees
                    || Math.Abs(c.Longitude - source.Longitude) > MaxDegrees)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts parsed response features into Syncfusion-ready geometry, filtered by the action
        /// toggles and (when supplied) the current playback time.
        /// </summary>
        private void BuildResponseGeometry()
        {
            var booms = new List<ResponseLine>();
            var shoreline = new List<ResponseLine>();
            var reconLines = new List<ResponseLine>();
            var dispersants = new List<List<Coordinate>>();
            var skimmers = new List<OilSpillMapPoint>();
            var recon = new List<OilSpillMapPoint>();

            foreach (var feature in _responseFeatures)
            {
                // Time-aware visibility: only hide a measure on the time axis when its own window
                // overlaps the simulation window. Response actions are often logged in real wall-
                // clock time that falls outside the modelled period; in that case the measure is
                // always shown (subject to its layer toggle) rather than being filtered away.
                if (CurrentTime is DateTime time
                    && FeatureOverlapsSimulation(feature)
                    && !feature.IsActiveAt(time))
                {
                    continue;
                }

                if (!IsActionTypeVisible(feature.ActionType))
                {
                    continue;
                }

                switch (feature.GeometryType)
                {
                    case "Point":
                        var point = feature.Coordinates[0];
                        var marker = new OilSpillMapPoint
                        {
                            Latitude = point.Latitude,
                            Longitude = point.Longitude,
                            Tooltip = feature.Tooltip
                        };
                        if (feature.ActionType == OilSpillActionType.Skimmer)
                        {
                            skimmers.Add(marker);
                        }
                        else
                        {
                            recon.Add(marker);
                        }
                        break;

                    case "LineString":
                        var line = ToResponseLine(feature.Coordinates);
                        switch (feature.ActionType)
                        {
                            case OilSpillActionType.DeployBoom:
                                booms.Add(line);
                                break;
                            case OilSpillActionType.ShorelineProtection:
                                shoreline.Add(line);
                                break;
                            default:
                                reconLines.Add(line);
                                break;
                        }
                        break;

                    case "Polygon":
                        var ring = feature.Coordinates
                            .Select(c => new Coordinate { Latitude = c.Latitude, Longitude = c.Longitude })
                            .ToList();
                        dispersants.Add(ring);
                        break;
                }
            }

            _boomLines = booms;
            _shorelineLines = shoreline;
            _reconLines = reconLines;
            _dispersantPolygons = dispersants;
            _skimmerMarkers = skimmers;
            _reconMarkers = recon;
        }

        private bool IsActionTypeVisible(OilSpillActionType actionType) => actionType switch
        {
            OilSpillActionType.DeployBoom => ShowBooms,
            OilSpillActionType.Skimmer => ShowSkimmers,
            OilSpillActionType.Dispersant => ShowDispersants,
            OilSpillActionType.ShorelineProtection => ShowShorelineProtection,
            _ => ShowActions
        };

        /// <summary>
        /// True when the response feature's active window overlaps the modelled trajectory period.
        /// Only such features participate in time-based show/hide during playback; features logged
        /// outside the simulated window are treated as always-on (subject to layer toggles).
        /// </summary>
        private bool FeatureOverlapsSimulation(OilSpillResponseFeature feature)
        {
            if (Trajectory.Count == 0)
            {
                return false;
            }

            var simStart = Trajectory[0].Timestamp;
            var simEnd = Trajectory[^1].Timestamp;
            var featureEnd = feature.EndTime ?? simEnd;

            return feature.StartTime <= simEnd && featureEnd >= simStart;
        }

        private static ResponseLine ToResponseLine(List<(double Latitude, double Longitude)> coords)
            => new()
            {
                Latitudes = coords.Select(c => c.Latitude).ToArray(),
                Longitudes = coords.Select(c => c.Longitude).ToArray()
            };

        private void ComputeCenter()
        {
            // Default to Algoa Bay if no usable coordinates are available.
            _centerLat = -33.96;
            _centerLon = 25.62;
            _zoomFactor = 11;

            // Centre and zoom are derived ONLY from the authoritative trajectory point coordinates
            // (real lat/lon persisted by the engine). Parsed slick-polygon geometry is deliberately
            // excluded here: a single malformed ring vertex could otherwise drag the bounding box
            // across the map and force the zoom right out.
            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLon = double.MaxValue, maxLon = double.MinValue;
            var any = false;

            foreach (var p in MapPoints)
            {
                if (!IsFinite(p.Latitude) || !IsFinite(p.Longitude))
                {
                    continue;
                }

                // Ignore obviously out-of-region points (Algoa Bay sits around -34, 25.6).
                if (Math.Abs(p.Latitude) > 90 || Math.Abs(p.Longitude) > 180)
                {
                    continue;
                }

                any = true;
                minLat = Math.Min(minLat, p.Latitude);
                maxLat = Math.Max(maxLat, p.Latitude);
                minLon = Math.Min(minLon, p.Longitude);
                maxLon = Math.Max(maxLon, p.Longitude);
            }

            if (!any)
            {
                return;
            }

            _centerLat = (minLat + maxLat) / 2.0;
            _centerLon = (minLon + maxLon) / 2.0;

            // Keep a stable tile-zoom that comfortably frames an Algoa Bay drift run. (Dynamic
            // zoom fitting against OSM tile layers mis-renders the tiles, so a fixed level is used.)
            _zoomFactor = 11;
        }

        private static bool IsFinite(double value)
            => !double.IsNaN(value) && !double.IsInfinity(value);

        /// <summary>
        /// Base colour for the slick, colour-coded by product type. Shoreline impact overrides
        /// this with alarm red.
        /// </summary>
        private (string Fill, string Border) ResolveSlickColors()
        {
            if (ShorelineImpact)
            {
                return ("rgba(220, 53, 69, 0.45)", "#dc3545");
            }

            return ProductType switch
            {
                // HFO -> Dark brown
                OilSpillProductType.HFO => ("rgba(74, 44, 23, 0.35)", "#4a2c17"),
                // VLSFO -> Dark grey
                OilSpillProductType.VLSFO => ("rgba(64, 64, 64, 0.35)", "#404040"),
                // MGO -> Light blue
                OilSpillProductType.MGO => ("rgba(135, 206, 235, 0.35)", "#3399cc"),
                // Crude -> Green
                OilSpillProductType.Crude => ("rgba(40, 167, 69, 0.35)", "#28a745"),
                // Chemicals -> Purple
                OilSpillProductType.Chemicals => ("rgba(128, 0, 128, 0.35)", "#800080"),
                _ => ("rgba(178, 76, 31, 0.30)", "#b24c1f")
            };
        }

        private string SlickFillColor => ResolveSlickColors().Fill;

        private string SlickBorderColor => ResolveSlickColors().Border;
    }
}
