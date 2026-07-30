using AlgoaBayBMT.Emergency.OilSpill.Models;

namespace AlgoaBayBMT.Emergency.OilSpill.Visualization
{
    /// <summary>
    /// A single point on the map polyline / marker layer.
    /// </summary>
    public class OilSpillMapPoint
    {
        public int Index { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public double AreaSqM { get; set; }
    }

    /// <summary>
    /// A marker for a response action (boom, skimmer, dispersant, etc).
    /// </summary>
    public class OilSpillActionMarker
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public OilSpillActionType ActionType { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
    }

    /// <summary>
    /// A response measure prepared for rendering: its geometry parsed into map-ready coordinates
    /// plus the metadata needed for styling and time-aware visibility.
    /// </summary>
    public class OilSpillResponseFeature
    {
        public OilSpillActionType ActionType { get; set; }

        /// <summary>"Point", "LineString", or "Polygon".</summary>
        public string GeometryType { get; set; } = string.Empty;

        /// <summary>Ordered coordinates (latitude, longitude) for the geometry.</summary>
        public List<(double Latitude, double Longitude)> Coordinates { get; set; } = new();

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;

        public bool IsActiveAt(DateTime time)
            => StartTime <= time && (EndTime is null || EndTime >= time);
    }

    /// <summary>
    /// A point used by the drift-distance and slick-area charts.
    /// </summary>
    public class OilSpillChartPoint
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string TimeLabel { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public double AreaSqM { get; set; }
        public double AreaSqKm => AreaSqM / 1_000_000.0;
    }

    /// <summary>
    /// Builds chart and map view-models from a list of trajectory points.
    /// </summary>
    public static class OilSpillVisualizationBuilder
    {
        public static List<OilSpillChartPoint> BuildChartPoints(IReadOnlyList<OilSpillTrajectoryPoint> trajectory)
        {
            var points = new List<OilSpillChartPoint>();
            if (trajectory.Count == 0)
            {
                return points;
            }

            var includeDate = OilSpillGeometry.RequiresDateLabels(trajectory);
            var origin = trajectory[0];

            for (var i = 0; i < trajectory.Count; i++)
            {
                var p = trajectory[i];
                var distance = OilSpillGeometry.HaversineDistance(
                    origin.Latitude, origin.Longitude, p.Latitude, p.Longitude);

                points.Add(new OilSpillChartPoint
                {
                    Index = i,
                    Timestamp = p.Timestamp,
                    TimeLabel = OilSpillGeometry.FormatTimestamp(p.Timestamp, includeDate),
                    DistanceKm = Math.Round(distance, 3),
                    AreaSqM = p.AreaSqM
                });
            }

            return points;
        }

        public static List<OilSpillMapPoint> BuildMapPoints(IReadOnlyList<OilSpillTrajectoryPoint> trajectory)
        {
            var points = new List<OilSpillMapPoint>();
            if (trajectory.Count == 0)
            {
                return points;
            }

            var includeDate = OilSpillGeometry.RequiresDateLabels(trajectory);

            for (var i = 0; i < trajectory.Count; i++)
            {
                var p = trajectory[i];
                var time = OilSpillGeometry.FormatTimestamp(p.Timestamp, includeDate);
                points.Add(new OilSpillMapPoint
                {
                    Index = i,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Label = time,
                    AreaSqM = p.AreaSqM,
                    Tooltip = $"t{i} · {time} · {(p.AreaSqM / 1_000_000.0):N3} km²"
                });
            }

            return points;
        }

        public static List<OilSpillActionMarker> BuildActionMarkers(IReadOnlyList<OilSpillResponseAction> actions)
        {
            var markers = new List<OilSpillActionMarker>();
            foreach (var action in actions)
            {
                if (action.Latitude is null || action.Longitude is null)
                {
                    continue;
                }

                markers.Add(new OilSpillActionMarker
                {
                    Latitude = action.Latitude.Value,
                    Longitude = action.Longitude.Value,
                    ActionType = action.ActionType,
                    Label = action.ActionType.ToString(),
                    Tooltip = $"{action.ActionType}: {action.Description}"
                });
            }

            return markers;
        }

        /// <summary>
        /// Parses each response action's geometry into a map-ready feature. Actions without usable
        /// geometry fall back to a Point built from their Latitude/Longitude when available.
        /// </summary>
        public static List<OilSpillResponseFeature> BuildResponseFeatures(
            IReadOnlyList<OilSpillResponseAction> actions)
        {
            var features = new List<OilSpillResponseFeature>();

            foreach (var action in actions)
            {
                var (type, coords) = OilSpillGeometry.ParseGeometry(action.GeometryGeoJson);

                // Fall back to a point from the stored coordinate when geometry is missing.
                if ((type is null || coords.Count == 0)
                    && action.Latitude is double lat && action.Longitude is double lon)
                {
                    type = "Point";
                    coords = new List<(double Latitude, double Longitude)> { (lat, lon) };
                }

                if (type is null || coords.Count == 0)
                {
                    continue;
                }

                features.Add(new OilSpillResponseFeature
                {
                    ActionType = action.ActionType,
                    GeometryType = type,
                    Coordinates = coords,
                    StartTime = action.StartTime,
                    EndTime = action.EndTime,
                    Description = action.Description,
                    Tooltip = $"{action.ActionType}: {action.Description}"
                });
            }

            return features;
        }
    }
}
