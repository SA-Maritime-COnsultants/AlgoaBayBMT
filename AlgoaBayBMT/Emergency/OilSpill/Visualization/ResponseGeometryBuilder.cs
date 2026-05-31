using System.Globalization;
using System.Text;

namespace AlgoaBayBMT.Emergency.OilSpill.Visualization
{
    /// <summary>
    /// Builds GeoJSON geometry strings (Point, LineString, Polygon) for oil-spill response
    /// measures. All coordinates are emitted in GeoJSON order, i.e. [longitude, latitude].
    /// </summary>
    public static class ResponseGeometryBuilder
    {
        private const double EarthRadiusMeters = 6378137.0;

        /// <summary>
        /// Builds a GeoJSON Point at the supplied coordinate.
        /// </summary>
        public static string BuildPointGeoJson(double lat, double lon)
        {
            var coordinate = $"[{Num(lon)},{Num(lat)}]";
            return $"{{\"type\":\"Point\",\"coordinates\":{coordinate}}}";
        }

        /// <summary>
        /// Builds a closed circular GeoJSON Polygon centred on the supplied coordinate with the
        /// supplied radius (metres). Used for skimmer/dispersant influence areas.
        /// </summary>
        public static string BuildCirclePolygonGeoJson(
            double centerLat, double centerLon, double radiusMeters, int segments = 36)
        {
            var safeRadius = Math.Max(radiusMeters, 1.0);
            var safeSegments = Math.Max(segments, 8);

            var cosLat = Math.Cos(DegToRad(centerLat));
            if (Math.Abs(cosLat) < 1e-6)
            {
                cosLat = cosLat < 0 ? -1e-6 : 1e-6;
            }

            var ring = new StringBuilder();
            ring.Append('[');

            for (var i = 0; i <= safeSegments; i++)
            {
                // Repeat the first vertex on the final iteration to close the ring.
                var index = i == safeSegments ? 0 : i;
                var theta = 2.0 * Math.PI * index / safeSegments;

                var east = safeRadius * Math.Cos(theta);
                var north = safeRadius * Math.Sin(theta);

                var lat = centerLat + RadToDeg(north / EarthRadiusMeters);
                var lon = centerLon + RadToDeg(east / (EarthRadiusMeters * cosLat));

                if (i > 0)
                {
                    ring.Append(',');
                }

                ring.Append('[').Append(Num(lon)).Append(',').Append(Num(lat)).Append(']');
            }

            ring.Append(']');

            return $"{{\"type\":\"Polygon\",\"coordinates\":[{ring}]}}";
        }

        /// <summary>
        /// Builds a GeoJSON LineString from an ordered list of (lat, lon) points. When fewer than
        /// two points are supplied a degenerate two-vertex line is produced so the result is always
        /// a valid LineString.
        /// </summary>
        public static string BuildLineStringGeoJson(List<(double lat, double lon)> points)
        {
            var coords = new StringBuilder();
            coords.Append('[');

            if (points is { Count: >= 2 })
            {
                for (var i = 0; i < points.Count; i++)
                {
                    if (i > 0)
                    {
                        coords.Append(',');
                    }

                    var (lat, lon) = points[i];
                    coords.Append('[').Append(Num(lon)).Append(',').Append(Num(lat)).Append(']');
                }
            }
            else if (points is { Count: 1 })
            {
                var (lat, lon) = points[0];
                coords.Append('[').Append(Num(lon)).Append(',').Append(Num(lat)).Append("],")
                      .Append('[').Append(Num(lon)).Append(',').Append(Num(lat)).Append(']');
            }
            else
            {
                coords.Append("[0,0],[0,0]");
            }

            coords.Append(']');

            return $"{{\"type\":\"LineString\",\"coordinates\":{coords}}}";
        }

        /// <summary>
        /// Builds a short GeoJSON LineString centred on (lat, lon), oriented along
        /// <paramref name="orientationDeg"/> (bearing clockwise from north). Used as a default boom
        /// footprint when only a single point is supplied.
        /// </summary>
        public static string BuildShortBoomGeoJson(
            double centerLat, double centerLon, double orientationDeg, double lengthMeters)
        {
            var half = Math.Max(lengthMeters, 1.0) / 2.0;
            var bearing = DegToRad(orientationDeg);

            var cosLat = Math.Cos(DegToRad(centerLat));
            if (Math.Abs(cosLat) < 1e-6)
            {
                cosLat = cosLat < 0 ? -1e-6 : 1e-6;
            }

            // Unit vector along the boom orientation (east, north components).
            var east = Math.Sin(bearing);
            var north = Math.Cos(bearing);

            var startLat = centerLat - RadToDeg(half * north / EarthRadiusMeters);
            var startLon = centerLon - RadToDeg(half * east / (EarthRadiusMeters * cosLat));
            var endLat = centerLat + RadToDeg(half * north / EarthRadiusMeters);
            var endLon = centerLon + RadToDeg(half * east / (EarthRadiusMeters * cosLat));

            return BuildLineStringGeoJson(new List<(double lat, double lon)>
            {
                (startLat, startLon),
                (endLat, endLon)
            });
        }

        /// <summary>
        /// Builds a short coastal LineString as a placeholder for shoreline-protection measures.
        /// Approximated as a fixed-length segment using a coarse Algoa Bay shoreline bearing
        /// (roughly NE-SW) until a real coastline reference is wired in.
        /// </summary>
        public static string BuildCoastalSegmentGeoJson(double lat, double lon)
        {
            // Algoa Bay shoreline trends roughly south-west to north-east (~035°).
            const double CoastlineBearingDeg = 35.0;
            const double DefaultCoastalLengthMeters = 400.0;
            return BuildShortBoomGeoJson(lat, lon, CoastlineBearingDeg, DefaultCoastalLengthMeters);
        }

        private static string Num(double value)
            => value.ToString("R", CultureInfo.InvariantCulture);

        private static double DegToRad(double degrees) => degrees * Math.PI / 180.0;

        private static double RadToDeg(double radians) => radians * 180.0 / Math.PI;
    }
}
