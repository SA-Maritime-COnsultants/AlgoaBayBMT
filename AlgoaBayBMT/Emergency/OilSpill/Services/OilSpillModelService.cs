using AlgoaBayBMT.Data;
using AlgoaBayBMT.Emergency.OilSpill.DTOs;
using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    /// <summary>
    /// Simple but credible Lagrangian drift modelling engine for oil-spill trajectory estimation.
    /// The model advects the spill centroid using a weighted combination of wind- and current-induced
    /// drift, and grows the surface area using a simple spreading model.
    /// </summary>
    public class OilSpillModelService : IOilSpillModelService
    {
        private const double EarthRadiusMeters = 6378137.0;
        private const double WindDriftFactor = 0.03;
        private const double CurrentDriftFactor = 1.0;
        private const int SlickSegments = 72;
        private const double MinAspectRatio = 1.5;
        private const double MaxAspectRatio = 6.0;

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ICoastlineService _coastline;
        private readonly IIncidentFormService _formService;

        public OilSpillModelService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ICoastlineService coastline,
            IIncidentFormService formService)
        {
            _contextFactory = contextFactory;
            _coastline = coastline;
            _formService = formService;
        }

        public async Task<OilSpillModelRun> RunModelAsync(OilSpillModelRunRequest request)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var spill = await context.OilSpillIncidents
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SpillId)
                ?? throw new InvalidOperationException($"Oil spill incident {request.SpillId} was not found.");

            // Load active response measures so the drift engine can apply their effects.
            var responseActions = await context.OilSpillResponseActions
                .Where(a => a.SpillId == request.SpillId)
                .AsNoTracking()
                .ToListAsync();

            OilSpillModelRun run;

            if (request.RunId is int runId)
            {
                // Edit an existing run: update its parameters and regenerate the trajectory.
                run = await context.OilSpillModelRuns
                    .FirstOrDefaultAsync(r => r.Id == runId && r.SpillId == request.SpillId)
                    ?? throw new InvalidOperationException($"Model run {runId} was not found.");

                run.RunName = string.IsNullOrWhiteSpace(request.RunName) ? "Base Case" : request.RunName;
                run.StartTime = request.StartTime;
                run.DurationHours = request.DurationHours;
                run.TimeStepMinutes = request.TimeStepMinutes <= 0 ? 30 : request.TimeStepMinutes;
                run.WindSpeed = request.WindSpeed;
                run.WindDirection = request.WindDirection;
                run.CurrentSpeed = request.CurrentSpeed;
                run.CurrentDirection = request.CurrentDirection;
                run.TideState = request.TideState;
                run.Notes = request.Notes;

                // Remove the previous trajectory so it can be regenerated with the new parameters.
                var existingPoints = await context.OilSpillTrajectoryPoints
                    .Where(t => t.ModelRunId == run.Id)
                    .ToListAsync();
                context.OilSpillTrajectoryPoints.RemoveRange(existingPoints);

                await context.SaveChangesAsync();
            }
            else
            {
                run = new OilSpillModelRun
                {
                    SpillId = request.SpillId,
                    RunName = string.IsNullOrWhiteSpace(request.RunName) ? "Base Case" : request.RunName,
                    StartTime = request.StartTime,
                    DurationHours = request.DurationHours,
                    TimeStepMinutes = request.TimeStepMinutes <= 0 ? 30 : request.TimeStepMinutes,
                    WindSpeed = request.WindSpeed,
                    WindDirection = request.WindDirection,
                    CurrentSpeed = request.CurrentSpeed,
                    CurrentDirection = request.CurrentDirection,
                    TideState = request.TideState,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                context.OilSpillModelRuns.Add(run);
                await context.SaveChangesAsync();
            }

            // Every run stores a Baseline (WITHOUT measures) prediction. When response measures
            // with usable geometry are deployed, a second Mitigated (WITH measures) prediction is
            // stored alongside it so the two scenarios can be compared before/after.
            var effects = ResponseEffects.FromActions(responseActions);

            var baseline = GenerateTrajectory(
                run, spill, OilSpillScenarioKind.Baseline, ResponseEffects.None, _coastline);
            run.ShorelineImpactIndex = baseline.ImpactIndex;
            run.ShorelineImpactTime = baseline.ImpactTime;
            run.ShorelineImpactGeoJson = baseline.ImpactZoneGeoJson;
            context.OilSpillTrajectoryPoints.AddRange(baseline.Points);

            run.HasMitigatedScenario = effects.HasAny;
            if (effects.HasAny)
            {
                var mitigated = GenerateTrajectory(
                    run, spill, OilSpillScenarioKind.Mitigated, effects, _coastline);
                run.MitigatedShorelineImpactIndex = mitigated.ImpactIndex;
                run.MitigatedShorelineImpactTime = mitigated.ImpactTime;
                run.MitigatedShorelineImpactGeoJson = mitigated.ImpactZoneGeoJson;
                context.OilSpillTrajectoryPoints.AddRange(mitigated.Points);
            }
            else
            {
                run.MitigatedShorelineImpactIndex = null;
                run.MitigatedShorelineImpactTime = null;
                run.MitigatedShorelineImpactGeoJson = null;
            }

            await context.SaveChangesAsync();

            // Automation: refresh the ICS-209 status summary from the latest run so the SITREP
            // always reflects current trajectory results.
            await _formService.SyncStatusSummaryAsync(request.SpillId);

            return run;
        }

        public async Task<OilSpillModelRun?> GetModelRunByIdAsync(int modelRunId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.OilSpillModelRuns
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == modelRunId);
        }

        public async Task<IReadOnlyList<OilSpillModelRun>> GetModelRunsForSpillAsync(int spillId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.OilSpillModelRuns
                .Where(r => r.SpillId == spillId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<IReadOnlyList<OilSpillTrajectoryPoint>> GetTrajectoryAsync(int modelRunId)
            => GetTrajectoryAsync(modelRunId, OilSpillScenarioKind.Baseline);

        public async Task<IReadOnlyList<OilSpillTrajectoryPoint>> GetTrajectoryAsync(
            int modelRunId, OilSpillScenarioKind scenario)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var points = await context.OilSpillTrajectoryPoints
                .Where(t => t.ModelRunId == modelRunId && t.Scenario == scenario)
                .OrderBy(t => t.Timestamp)
                .AsNoTracking()
                .ToListAsync();

            // Runs generated before the dual-scenario model stored a single unlabelled point set
            // (persisted as Baseline). Fall back to it when a Mitigated set was requested but not
            // stored, so older runs still visualise.
            if (points.Count == 0 && scenario == OilSpillScenarioKind.Mitigated)
            {
                points = await context.OilSpillTrajectoryPoints
                    .Where(t => t.ModelRunId == modelRunId
                                && t.Scenario == OilSpillScenarioKind.Baseline)
                    .OrderBy(t => t.Timestamp)
                    .AsNoTracking()
                    .ToListAsync();
            }

            return points;
        }

        public async Task<bool> DeleteModelRunAsync(int modelRunId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var run = await context.OilSpillModelRuns.FirstOrDefaultAsync(r => r.Id == modelRunId);
            if (run is null)
            {
                return false;
            }

            // Trajectory points are removed via cascade delete.
            context.OilSpillModelRuns.Remove(run);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task RecordShorelineImpactAsync(int modelRunId, int impactIndex, DateTime impactTime)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var run = await context.OilSpillModelRuns.FirstOrDefaultAsync(r => r.Id == modelRunId);
            if (run is null)
            {
                return;
            }

            run.ShorelineImpactIndex = impactIndex;
            run.ShorelineImpactTime = impactTime;
            await context.SaveChangesAsync();
        }

        /// <summary>The result of generating one scenario's trajectory.</summary>
        private sealed record ScenarioResult(
            List<OilSpillTrajectoryPoint> Points,
            int? ImpactIndex,
            DateTime? ImpactTime,
            string? ImpactZoneGeoJson);

        // After landfall the seaward drift stops; the slick then spreads along the coastline for
        // up to this many hours (bounded by the remaining run duration).
        private const double PostImpactSpreadHours = 6.0;
        // Fractional area growth per hour of alongshore spreading, capped at 3x the impact area.
        private const double PostImpactSpreadRatePerHour = 0.45;
        private const double PostImpactMaxGrowth = 3.0;

        private static ScenarioResult GenerateTrajectory(
            OilSpillModelRun run,
            OilSpillIncident spill,
            OilSpillScenarioKind scenario,
            ResponseEffects effects,
            ICoastlineService? coastline)
        {
            var points = new List<OilSpillTrajectoryPoint>();

            var timeStepMinutes = run.TimeStepMinutes <= 0 ? 30 : run.TimeStepMinutes;
            var totalMinutes = run.DurationHours * 60.0;
            var stepCount = Math.Max(1, (int)Math.Floor(totalMinutes / timeStepMinutes));
            var stepSeconds = timeStepMinutes * 60.0;

            // Resolve wind and current vectors (meters/second) from speed + direction.
            // Direction is the bearing the vector points TOWARDS, in degrees clockwise from north.
            var (windX, windY) = ToVector(run.WindSpeed, run.WindDirection);
            var (baseCurrentX, baseCurrentY) = ToVector(run.CurrentSpeed, run.CurrentDirection);

            // Cross-current unit vector (perpendicular, rotated +90°) used to inject the
            // back-and-forth tidal excursion that makes the path meander instead of being a
            // perfectly straight line.
            var currentMag = Math.Sqrt((baseCurrentX * baseCurrentX) + (baseCurrentY * baseCurrentY));
            double crossX = 0.0, crossY = 0.0;
            if (currentMag > 1e-6)
            {
                crossX = -baseCurrentY / currentMag;
                crossY = baseCurrentX / currentMag;
            }

            // Viscosity-based spreading: light products spread more, heavy products less.
            var spreadFactor = SpreadFactorFor(spill.ProductType);

            var baseArea = Math.Max(1000.0, spill.EstimatedVolume * 5.0);

            var currentLat = spill.Latitude;
            var currentLon = spill.Longitude;

            // Running cumulative footprint (union of every instantaneous slick polygon so far).
            string? cumulativePolygon = null;

            // First-landfall tracking for this scenario.
            int? impactIndex = null;
            DateTime? impactTime = null;

            // Semi-diurnal tide period (~12.42 h) drives the oscillating tidal stream.
            const double TidalPeriodSeconds = 12.42 * 3600.0;
            // Tidal stream amplitude scales with the supplied current speed.
            var tidalAmplitude = Math.Max(0.15, run.CurrentSpeed * 0.6);
            // Slow steady veer of the wind-driven component (Coriolis-like turning), radians/sec.
            const double WindVeerRatePerHour = 4.0 * Math.PI / 180.0; // ~4°/h
            var windVeerRatePerSec = WindVeerRatePerHour / 3600.0;

            for (var step = 0; step <= stepCount; step++)
            {
                var elapsedSeconds = step * stepSeconds;
                var timestamp = run.StartTime.AddSeconds(elapsedSeconds);
                var area = baseArea * (1.0 + Math.Sqrt(step));

                // --- Time-varying environment for this step ---
                // Tidal phase oscillates the along/cross current to produce a natural meander.
                var tidalPhase = 2.0 * Math.PI * elapsedSeconds / TidalPeriodSeconds;
                var tidalAlong = Math.Cos(tidalPhase);          // strengthens/reverses the stream
                var tidalCross = tidalAmplitude * Math.Sin(tidalPhase); // sideways excursion

                var currentX = (baseCurrentX * (0.6 + (0.4 * tidalAlong))) + (crossX * tidalCross);
                var currentY = (baseCurrentY * (0.6 + (0.4 * tidalAlong))) + (crossY * tidalCross);

                // Wind drift slowly veers over time (Coriolis-like rotation of surface drift).
                var veer = windVeerRatePerSec * elapsedSeconds;
                var cosV = Math.Cos(veer);
                var sinV = Math.Sin(veer);
                var windDriftX = (windX * cosV) - (windY * sinV);
                var windDriftY = (windX * sinV) + (windY * cosV);

                // Combined instantaneous drift velocity (m/s).
                var driftX = (windDriftX * WindDriftFactor) + (currentX * CurrentDriftFactor);
                var driftY = (windDriftY * WindDriftFactor) + (currentY * CurrentDriftFactor);

                // Resultant drift bearing for the slick's elongation this step.
                var driftDirectionDeg = RadToDeg(Math.Atan2(driftX, driftY));

                // Dynamic aspect ratio: stronger wind/current stretches the slick further along
                // the drift axis. Clamped to keep the teardrop physically plausible.
                var stepDriftSpeed = Math.Sqrt((driftX * driftX) + (driftY * driftY));
                var aspectRatio = Math.Clamp(
                    1.5 + (run.WindSpeed * 0.15) + (stepDriftSpeed * 0.6),
                    MinAspectRatio,
                    MaxAspectRatio);

                // Time-dependent spreading: oil spreads quickly at first then slows.
                var timeFactor = Math.Log(1.0 + step);
                var effectiveArea = area * spreadFactor * Math.Max(timeFactor, 1.0);

                // The slick ring is needed for the response-measure interaction tests, so compute
                // it once up-front for this step.
                var slick = OilSpillGeometry.ParsePolygon(BuildSlickPolygonGeoJson(
                    currentLat, currentLon, effectiveArea, driftDirectionDeg, aspectRatio));

                // True when an active boom or shoreline-protection measure is currently containing
                // the slick; used to delay shoreline-impact detection for this step.
                var shorelineImpactDelayed = false;

                // --- Apply simplified physical effects of active response measures ---
                if (effects.HasAny && slick.Count >= 3)
                {
                    // Dispersant: slows further spreading where the slick is treated and speeds
                    // weathering/decay so the reported area shrinks faster.
                    if (effects.IntersectsDispersant(slick, timestamp))
                    {
                        effectiveArea *= 0.5;
                        area *= 0.85;
                    }

                    // Skimmer: actively recovers oil, reducing the reported area.
                    var recovered = effects.SkimmerRecovery(slick, timestamp, stepSeconds);
                    if (recovered > 0)
                    {
                        area = Math.Max(0.0, area - recovered);
                        effectiveArea = Math.Max(1.0, effectiveArea - recovered);
                    }

                    // Boom: contains the slick, damping drift on the far side and delaying the
                    // moment it reaches the shoreline.
                    if (effects.IntersectsBoom(slick, timestamp))
                    {
                        driftX *= 0.2;
                        driftY *= 0.2;
                        shorelineImpactDelayed = true;
                    }

                    // Shoreline protection: a protected segment prevents/defers shoreline impact.
                    if (effects.IsShorelineProtected(slick, timestamp))
                    {
                        shorelineImpactDelayed = true;
                    }
                }

                // Build the instantaneous slick polygon (unclipped) for this step. It is used both
                // for landfall detection against the coastline and, after clipping to water, for
                // rendering.
                var rawSlickGeoJson = BuildSlickPolygonGeoJson(
                    currentLat, currentLon, effectiveArea, driftDirectionDeg, aspectRatio);

                // --- Landfall detection (polygon intersection with coastline) ---
                // When the UNCLIPPED slick reaches the shore (and no active response measure is
                // currently delaying impact) resolve the exact shoreline-impact location so the
                // plume, impact marker and reported position all sit on the coast rather than on
                // the drifting centroid.
                (double Latitude, double Longitude)? landfall = null;
                if (!shorelineImpactDelayed && coastline is not null && coastline.HasCoastline)
                {
                    landfall = coastline.LandfallPoint(rawSlickGeoJson);
                }

                // Clip the slick to water so it stops at (and spreads along) the coastline instead
                // of bleeding inland. Falls back to the raw polygon when no coastline is loaded.
                var instantaneousPolygon = coastline is not null
                    ? coastline.ClipToWater(rawSlickGeoJson)
                    : rawSlickGeoJson;

                cumulativePolygon = coastline is not null
                    ? coastline.Union(cumulativePolygon, instantaneousPolygon)
                    : instantaneousPolygon;

                // On landfall, snap the recorded position to the shoreline-impact point so the
                // trajectory ends exactly where the slick meets the coast.
                var pointLat = landfall?.Latitude ?? currentLat;
                var pointLon = landfall?.Longitude ?? currentLon;

                points.Add(new OilSpillTrajectoryPoint
                {
                    ModelRunId = run.Id,
                    Scenario = scenario,
                    Timestamp = timestamp,
                    Latitude = pointLat,
                    Longitude = pointLon,
                    AreaSqM = area,
                    ThicknessMm = null,
                    PolygonGeoJson = instantaneousPolygon,
                    CumulativePolygonGeoJson = cumulativePolygon
                });

                // First shoreline impact: seaward drift STOPS here. The oil does not keep drifting
                // out at sea; instead it spreads along the coastline, so a bounded number of
                // post-impact steps grow the slick isotropically at the landfall point and clip it
                // to water — the visible footprint then elongates naturally along the shore.
                if (landfall is not null)
                {
                    impactIndex = points.Count - 1;
                    impactTime = timestamp;

                    cumulativePolygon = AppendAlongshoreSpreading(
                        points, run, scenario, effects, coastline,
                        landfall.Value.Latitude, landfall.Value.Longitude,
                        effectiveArea, area, timestamp, stepSeconds,
                        remainingSteps: stepCount - step,
                        timeStepMinutes, cumulativePolygon);
                    break;
                }

                // Advect the centroid for the next step using this step's instantaneous drift.
                var dx = driftX * stepSeconds; // meters east
                var dy = driftY * stepSeconds; // meters north

                var dLat = dy / EarthRadiusMeters;
                var dLon = dx / (EarthRadiusMeters * Math.Cos(DegToRad(currentLat)));

                currentLat += RadToDeg(dLat);
                currentLon += RadToDeg(dLon);
            }

            // Resolve the impacted stretch of coastline (drawn as the shoreline-impact zone).
            var impactZone = impactIndex is not null && coastline is not null
                ? coastline.ImpactZone(cumulativePolygon)
                : null;

            return new ScenarioResult(points, impactIndex, impactTime, impactZone);
        }

        /// <summary>
        /// Models alongshore spreading after landfall: the centroid stays pinned at the landfall
        /// point (drift has stopped), while the slick area keeps growing at a damped rate and is
        /// clipped to water each step so the footprint spreads along the coastline. An active
        /// shoreline-protection measure halves the spreading rate. Returns the updated cumulative
        /// footprint.
        /// </summary>
        private static string? AppendAlongshoreSpreading(
            List<OilSpillTrajectoryPoint> points,
            OilSpillModelRun run,
            OilSpillScenarioKind scenario,
            ResponseEffects effects,
            ICoastlineService? coastline,
            double impactLat,
            double impactLon,
            double impactEffectiveArea,
            double impactArea,
            DateTime impactTimestamp,
            double stepSeconds,
            int remainingSteps,
            double timeStepMinutes,
            string? cumulativePolygon)
        {
            var spreadSteps = Math.Min(
                Math.Max(0, remainingSteps),
                (int)Math.Ceiling(PostImpactSpreadHours * 60.0 / Math.Max(1.0, timeStepMinutes)));

            for (var s = 1; s <= spreadSteps; s++)
            {
                var timestamp = impactTimestamp.AddSeconds(s * stepSeconds);
                var hoursSince = s * stepSeconds / 3600.0;

                var rate = PostImpactSpreadRatePerHour;

                // A deployed shoreline-protection measure at the impacted coast slows the
                // alongshore spreading (oil is deflected/recovered at the barrier).
                var probeRing = OilSpillGeometry.GenerateCirclePolygon(
                    impactLat, impactLon,
                    OilSpillGeometry.RadiusFromAreaMeters(impactEffectiveArea), 24);
                if (effects.HasAny && effects.IsShorelineProtected(probeRing, timestamp))
                {
                    rate *= 0.5;
                }

                var growth = Math.Min(1.0 + (rate * hoursSince), PostImpactMaxGrowth);
                var spreadEffectiveArea = impactEffectiveArea * growth;
                var spreadArea = impactArea * growth;

                // Isotropic (aspect 1) slick at the landfall point; clipping to water shapes it
                // along the coastline in both directions.
                var circleGeoJson = BuildSlickPolygonGeoJson(
                    impactLat, impactLon, spreadEffectiveArea, driftDirectionDeg: 0.0, aspectRatio: 1.0);

                var clipped = coastline is not null
                    ? coastline.ClipToWater(circleGeoJson)
                    : circleGeoJson;

                cumulativePolygon = coastline is not null
                    ? coastline.Union(cumulativePolygon, clipped)
                    : clipped;

                points.Add(new OilSpillTrajectoryPoint
                {
                    ModelRunId = run.Id,
                    Scenario = scenario,
                    Timestamp = timestamp,
                    Latitude = impactLat,
                    Longitude = impactLon,
                    AreaSqM = spreadArea,
                    ThicknessMm = null,
                    PolygonGeoJson = clipped,
                    CumulativePolygonGeoJson = cumulativePolygon
                });
            }

            return cumulativePolygon;
        }

        /// <summary>
        /// Builds a GeoJSON Polygon string approximating an oil slick as a teardrop whose pointed
        /// tail trails downwind/down-current along the resultant drift direction. The shape area is
        /// driven by <paramref name="areaSqM"/> and elongated using <paramref name="aspectRatio"/>.
        /// </summary>
        /// <param name="centerLat">Slick centroid latitude in degrees.</param>
        /// <param name="centerLon">Slick centroid longitude in degrees.</param>
        /// <param name="areaSqM">Effective slick surface area in square metres.</param>
        /// <param name="driftDirectionDeg">Drift bearing in degrees clockwise from north.</param>
        /// <param name="aspectRatio">Major-to-minor axis ratio (drift-driven elongation).</param>
        private static string BuildSlickPolygonGeoJson(
            double centerLat, double centerLon, double areaSqM, double driftDirectionDeg, double aspectRatio)
        {
            // Use an equivalent ellipse (area = π * a * b, a = b * aspectRatio) to scale the
            // teardrop so it covers roughly the requested area regardless of elongation.
            var safeArea = Math.Max(areaSqM, 1.0);
            var safeAspect = Math.Max(aspectRatio, 1.0);
            var a = Math.Sqrt(safeArea * safeAspect / Math.PI); // semi-major axis (m), drift axis
            var b = a / safeAspect;                             // semi-minor axis (m), cross-drift

            var phi = DegToRad(driftDirectionDeg);
            var cosPhi = Math.Cos(phi);
            var sinPhi = Math.Sin(phi);
            var cosLat = Math.Cos(DegToRad(centerLat));
            if (Math.Abs(cosLat) < 1e-6)
            {
                cosLat = cosLat < 0 ? -1e-6 : 1e-6;
            }

            var coordinates = new System.Text.StringBuilder();
            coordinates.Append('[');

            for (var i = 0; i <= SlickSegments; i++)
            {
                // Close the ring by repeating the first vertex on the final iteration.
                var index = i == SlickSegments ? 0 : i;
                var theta = 2.0 * Math.PI * index / SlickSegments;

                // Teardrop radius profile. θ = 0 points downwind (the pointed tail), θ = π is the
                // rounded upwind head. cos(θ) smoothly blends the two extremes around the sides.
                var c = Math.Cos(theta);
                double radius;
                if (c >= 0.0)
                {
                    // Downwind half: stretch the tail with the major axis.
                    radius = a * (0.6 + (0.4 * c));
                }
                else
                {
                    // Upwind half: rounded head using the minor axis.
                    radius = b * (1.0 + (0.5 * c));
                }
                radius = Math.Max(radius, b * 0.2);

                // Local teardrop coordinates: x along drift axis, y across it.
                var x = radius * Math.Cos(theta);
                var y = (b / a) * radius * Math.Sin(theta);

                // Rotate by the drift bearing. With x'=east, y'=north and a bearing measured
                // clockwise from north, east' = x*cosφ + y*sinφ and north' = -x*sinφ + y*cosφ
                // aligns the major (tail) axis with the drift direction.
                var east = (x * cosPhi) + (y * sinPhi);
                var north = (-x * sinPhi) + (y * cosPhi);

                var dLat = north / EarthRadiusMeters;
                var dLon = east / (EarthRadiusMeters * cosLat);

                var lat = centerLat + RadToDeg(dLat);
                var lon = centerLon + RadToDeg(dLon);

                if (i > 0)
                {
                    coordinates.Append(',');
                }

                coordinates.Append('[')
                    .Append(lon.ToString("R", System.Globalization.CultureInfo.InvariantCulture))
                    .Append(',')
                    .Append(lat.ToString("R", System.Globalization.CultureInfo.InvariantCulture))
                    .Append(']');
            }

            coordinates.Append(']');

            return $"{{\"type\":\"Polygon\",\"coordinates\":[{coordinates}]}}";
        }

        /// <summary>
        /// Viscosity-based spreading multiplier. Lighter products (MGO, chemicals) spread further
        /// than heavy, viscous products (HFO).
        /// </summary>
        private static double SpreadFactorFor(OilSpillProductType productType) => productType switch
        {
            OilSpillProductType.HFO => 0.4,
            OilSpillProductType.VLSFO => 0.6,
            OilSpillProductType.MGO => 1.0,
            OilSpillProductType.Crude => 0.7,
            OilSpillProductType.Chemicals => 1.2,
            _ => 0.8
        };

        private static (double X, double Y) ToVector(double speed, double directionDegrees)
        {
            var rad = DegToRad(directionDegrees);
            // X = east component, Y = north component.
            var x = speed * Math.Sin(rad);
            var y = speed * Math.Cos(rad);
            return (x, y);
        }

        private static double DegToRad(double degrees) => degrees * Math.PI / 180.0;

        private static double RadToDeg(double radians) => radians * 180.0 / Math.PI;
    }
}
