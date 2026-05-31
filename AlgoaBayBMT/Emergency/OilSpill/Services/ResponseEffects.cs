using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    /// <summary>
    /// Precomputed response-measure geometries and the simplified physical effects they apply to a
    /// drifting slick (boom containment, dispersant treatment, skimmer recovery). Geometry is parsed
    /// once and reused for every timestep of a model run.
    /// </summary>
    internal sealed class ResponseEffects
    {
        // Approximate oil recovery rate per active skimmer, in square metres per second.
        private const double SkimmerRecoveryRateSqmPerSec = 0.5;

        private readonly List<TimedGeometry> _booms = new();
        private readonly List<TimedGeometry> _dispersants = new();
        private readonly List<TimedGeometry> _skimmers = new();
        private readonly List<TimedGeometry> _shorelineProtection = new();

        public bool HasAny => _booms.Count > 0 || _dispersants.Count > 0 || _skimmers.Count > 0
            || _shorelineProtection.Count > 0;

        public static ResponseEffects FromActions(IReadOnlyList<OilSpillResponseAction>? actions)
        {
            var effects = new ResponseEffects();
            if (actions is null)
            {
                return effects;
            }

            foreach (var action in actions)
            {
                switch (action.ActionType)
                {
                    case OilSpillActionType.DeployBoom:
                        var boomLine = ParseLine(action);
                        if (boomLine.Count >= 2)
                        {
                            effects._booms.Add(new TimedGeometry(boomLine, action.StartTime, action.EndTime));
                        }
                        break;

                    case OilSpillActionType.Dispersant:
                        var ring = OilSpillGeometry.ParsePolygon(action.GeometryGeoJson);
                        if (ring.Count >= 3)
                        {
                            effects._dispersants.Add(new TimedGeometry(ring, action.StartTime, action.EndTime));
                        }
                        break;

                    case OilSpillActionType.Skimmer:
                        if (action.Latitude is double lat && action.Longitude is double lon)
                        {
                            var radius = action.RadiusMeters ?? 150.0;
                            var circle = OilSpillGeometry.GenerateCirclePolygon(lat, lon, radius, 36);
                            if (circle.Count >= 3)
                            {
                                effects._skimmers.Add(
                                    new TimedGeometry(circle, action.StartTime, action.EndTime));
                            }
                        }
                        break;

                    case OilSpillActionType.ShorelineProtection:
                        var protectionLine = ParseLine(action);
                        if (protectionLine.Count >= 2)
                        {
                            effects._shorelineProtection.Add(
                                new TimedGeometry(protectionLine, action.StartTime, action.EndTime));
                        }
                        break;
                }
            }

            return effects;
        }

        public bool IntersectsBoom(List<(double Latitude, double Longitude)> slick, DateTime time)
        {
            foreach (var boom in _booms)
            {
                if (boom.IsActiveAt(time) && OilSpillGeometry.PolygonIntersectsLine(slick, boom.Coordinates))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IntersectsDispersant(List<(double Latitude, double Longitude)> slick, DateTime time)
        {
            foreach (var dispersant in _dispersants)
            {
                if (dispersant.IsActiveAt(time)
                    && OilSpillGeometry.PolygonIntersectsPolygon(slick, dispersant.Coordinates))
                {
                    return true;
                }
            }

            return false;
        }

        public double SkimmerRecovery(
            List<(double Latitude, double Longitude)> slick, DateTime time, double stepSeconds)
        {
            var recovered = 0.0;
            foreach (var skimmer in _skimmers)
            {
                if (skimmer.IsActiveAt(time)
                    && OilSpillGeometry.PolygonIntersectsPolygon(slick, skimmer.Coordinates))
                {
                    recovered += SkimmerRecoveryRateSqmPerSec * stepSeconds;
                }
            }

            return recovered;
        }

        /// <summary>
        /// Returns true when an active shoreline-protection measure covers the impacted slick,
        /// indicating the shoreline impact should be suppressed/mitigated for this step.
        /// </summary>
        public bool IsShorelineProtected(List<(double Latitude, double Longitude)> slick, DateTime time)
        {
            foreach (var protection in _shorelineProtection)
            {
                if (protection.IsActiveAt(time)
                    && OilSpillGeometry.PolygonIntersectsLine(slick, protection.Coordinates))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<(double Latitude, double Longitude)> ParseLine(OilSpillResponseAction action)
        {
            var (_, coords) = OilSpillGeometry.ParseGeometry(action.GeometryGeoJson);
            return coords;
        }

        private sealed class TimedGeometry
        {
            public TimedGeometry(
                List<(double Latitude, double Longitude)> coordinates, DateTime start, DateTime? end)
            {
                Coordinates = coordinates;
                Start = start;
                End = end;
            }

            public List<(double Latitude, double Longitude)> Coordinates { get; }
            public DateTime Start { get; }
            public DateTime? End { get; }

            public bool IsActiveAt(DateTime time) => Start <= time && (End is null || End >= time);
        }
    }
}
