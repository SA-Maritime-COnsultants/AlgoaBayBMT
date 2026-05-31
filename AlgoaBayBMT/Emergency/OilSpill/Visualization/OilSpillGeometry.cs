using System.Globalization;
using System.Text.Json;
using AlgoaBayBMT.Emergency.OilSpill.Models;

namespace AlgoaBayBMT.Emergency.OilSpill.Visualization
{
    /// <summary>
    /// Stateless helper methods used by the oil spill visualisation components.
    /// </summary>
    public static class OilSpillGeometry
    {
        private const double EarthRadiusKm = 6371.0088;

        /// <summary>
        /// Great-circle distance in kilometres between two WGS-84 coordinates.
        /// </summary>
        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = DegToRad(lat2 - lat1);
            var dLon = DegToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2))
                    * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Formats a timestamp using "HH:mm" for short runs and "dd MMM HH:mm" for runs
        /// that span more than 24 hours.
        /// </summary>
        public static string FormatTimestamp(DateTime timestamp, bool includeDate = false)
            => includeDate
                ? timestamp.ToString("dd MMM HH:mm", CultureInfo.InvariantCulture)
                : timestamp.ToString("HH:mm", CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns true if the trajectory spans more than 24 hours, in which case
        /// date-qualified timestamps should be used.
        /// </summary>
        public static bool RequiresDateLabels(IReadOnlyList<OilSpillTrajectoryPoint> trajectory)
        {
            if (trajectory.Count < 2)
            {
                return false;
            }

            var span = trajectory[^1].Timestamp - trajectory[0].Timestamp;
            return span.TotalHours > 24;
        }

        /// <summary>
        /// Parses a GeoJSON Polygon (or the first ring of a MultiPolygon) into a list of
        /// latitude/longitude points suitable for a Syncfusion polygon layer.
        /// Returns an empty list when the input is null, empty, or not a polygon.
        /// </summary>
        public static List<(double Latitude, double Longitude)> ParsePolygon(string? polygonGeoJson)
        {
            var result = new List<(double Latitude, double Longitude)>();
            if (string.IsNullOrWhiteSpace(polygonGeoJson))
            {
                return result;
            }

            try
            {
                using var document = JsonDocument.Parse(polygonGeoJson);
                var root = document.RootElement;

                // Support either a bare geometry object or a Feature wrapper.
                var geometry = root;
                if (root.TryGetProperty("geometry", out var geom))
                {
                    geometry = geom;
                }

                if (!geometry.TryGetProperty("type", out var typeElement) ||
                    !geometry.TryGetProperty("coordinates", out var coordinates))
                {
                    return result;
                }

                var type = typeElement.GetString();

                JsonElement ring;
                if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    // coordinates: [ [ [lon,lat], ... ] ]
                    ring = coordinates[0];
                }
                else if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
                {
                    // coordinates: [ [ [ [lon,lat], ... ] ] ]
                    ring = coordinates[0][0];
                }
                else
                {
                    return result;
                }

                foreach (var pair in ring.EnumerateArray())
                {
                    // GeoJSON stores [longitude, latitude].
                    var lon = pair[0].GetDouble();
                    var lat = pair[1].GetDouble();
                    result.Add((lat, lon));
                }
            }
            catch (JsonException)
            {
                // Malformed GeoJSON - return whatever was parsed (possibly empty).
            }

            return result;
        }

        /// <summary>
        /// Parses a GeoJSON Polygon or MultiPolygon into one or more outer rings (lat/lon). Each
        /// ring can be rendered as a separate Syncfusion polygon. Holes are ignored. Returns an
        /// empty list when the input is null/empty or not a (multi)polygon.
        /// </summary>
        public static List<List<(double Latitude, double Longitude)>> ParsePolygonRings(string? polygonGeoJson)
        {
            var rings = new List<List<(double Latitude, double Longitude)>>();
            if (string.IsNullOrWhiteSpace(polygonGeoJson))
            {
                return rings;
            }

            try
            {
                using var document = JsonDocument.Parse(polygonGeoJson);
                var root = document.RootElement;

                var geometry = root;
                if (root.TryGetProperty("geometry", out var geom))
                {
                    geometry = geom;
                }

                if (!geometry.TryGetProperty("type", out var typeElement) ||
                    !geometry.TryGetProperty("coordinates", out var coordinates))
                {
                    return rings;
                }

                var type = typeElement.GetString();

                if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    // coordinates: [ outerRing, hole1, ... ] -> take the outer ring only.
                    AddRing(rings, coordinates[0]);
                }
                else if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
                {
                    // coordinates: [ [ outerRing, holes... ], ... ] -> outer ring of each polygon.
                    foreach (var polygon in coordinates.EnumerateArray())
                    {
                        AddRing(rings, polygon[0]);
                    }
                }
            }
            catch (JsonException)
            {
                // Malformed GeoJSON - return whatever was parsed.
            }

            return rings;
        }

        private static void AddRing(
            List<List<(double Latitude, double Longitude)>> rings, JsonElement ring)
        {
            var points = new List<(double Latitude, double Longitude)>();
            foreach (var pair in ring.EnumerateArray())
            {
                // GeoJSON stores [longitude, latitude].
                points.Add((pair[1].GetDouble(), pair[0].GetDouble()));
            }

            if (points.Count >= 3)
            {
                rings.Add(points);
            }
        }

        /// <summary>
        /// Parses a GeoJSON geometry of type Point, LineString, or Polygon into its geometry type
        /// and an ordered list of (latitude, longitude) coordinates. Returns a null type and empty
        /// list when the input is null/empty/unsupported.
        /// </summary>
        public static (string? Type, List<(double Latitude, double Longitude)> Coordinates) ParseGeometry(
            string? geoJson)
        {
            var coords = new List<(double Latitude, double Longitude)>();
            if (string.IsNullOrWhiteSpace(geoJson))
            {
                return (null, coords);
            }

            try
            {
                using var document = JsonDocument.Parse(geoJson);
                var root = document.RootElement;

                var geometry = root;
                if (root.TryGetProperty("geometry", out var geom))
                {
                    geometry = geom;
                }

                if (!geometry.TryGetProperty("type", out var typeElement) ||
                    !geometry.TryGetProperty("coordinates", out var coordinates))
                {
                    return (null, coords);
                }

                var type = typeElement.GetString();

                if (string.Equals(type, "Point", StringComparison.OrdinalIgnoreCase))
                {
                    // coordinates: [lon, lat]
                    coords.Add((coordinates[1].GetDouble(), coordinates[0].GetDouble()));
                }
                else if (string.Equals(type, "LineString", StringComparison.OrdinalIgnoreCase))
                {
                    // coordinates: [ [lon,lat], ... ]
                    foreach (var pair in coordinates.EnumerateArray())
                    {
                        coords.Add((pair[1].GetDouble(), pair[0].GetDouble()));
                    }
                }
                else if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    // coordinates: [ [ [lon,lat], ... ] ]
                    foreach (var pair in coordinates[0].EnumerateArray())
                    {
                        coords.Add((pair[1].GetDouble(), pair[0].GetDouble()));
                    }
                }
                else
                {
                    return (null, coords);
                }

                return (type, coords);
            }
            catch (JsonException)
            {
                return (null, coords);
            }
        }

        /// <summary>
        /// Returns true when a polygon ring (lat/lon) intersects or contains a line defined by an
        /// ordered list of points. Used for simple boom/dispersant/skimmer interaction tests.
        /// </summary>
        public static bool PolygonIntersectsLine(
            List<(double Latitude, double Longitude)> polygon,
            List<(double Latitude, double Longitude)> line)
        {
            if (polygon.Count < 3 || line.Count < 2)
            {
                return false;
            }

            // Any line vertex inside the polygon counts as an intersection.
            foreach (var p in line)
            {
                if (PointInPolygon(p, polygon))
                {
                    return true;
                }
            }

            // Otherwise test segment crossings between the line and polygon edges.
            for (var i = 0; i < line.Count - 1; i++)
            {
                for (var j = 0; j < polygon.Count - 1; j++)
                {
                    if (SegmentsIntersect(line[i], line[i + 1], polygon[j], polygon[j + 1]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <param name="centerLat">Centre latitude in degrees.</param>
        /// <param name="centerLon">Centre longitude in degrees.</param>
        /// <param name="radiusMeters">Circle radius in metres.</param>
        /// <param name="segments">Number of segments approximating the circle (default 72).</param>
        public static List<(double Latitude, double Longitude)> GenerateCirclePolygon(
            double centerLat, double centerLon, double radiusMeters, int segments = 72)
        {
            var points = new List<(double Latitude, double Longitude)>();
            if (radiusMeters <= 0 || segments < 3
                || double.IsNaN(centerLat) || double.IsNaN(centerLon)
                || double.IsInfinity(centerLat) || double.IsInfinity(centerLon))
            {
                return points;
            }

            var latRad = DegToRad(centerLat);

            // Metres-per-degree using the WGS-84 ellipsoidal approximation.
            var metersPerDegLat = 111_132.92
                                  - 559.82 * Math.Cos(2 * latRad)
                                  + 1.175 * Math.Cos(4 * latRad);
            var metersPerDegLon = 111_412.84 * Math.Cos(latRad)
                                  - 93.5 * Math.Cos(3 * latRad);

            if (metersPerDegLat <= 0)
            {
                metersPerDegLat = 111_132.92;
            }

            // Guard against division blow-up close to the poles.
            if (Math.Abs(metersPerDegLon) < 1.0)
            {
                metersPerDegLon = metersPerDegLon < 0 ? -1.0 : 1.0;
            }

            for (var i = 0; i <= segments; i++)
            {
                var theta = 2 * Math.PI * i / segments;
                var dLat = radiusMeters * Math.Cos(theta) / metersPerDegLat;
                var dLon = radiusMeters * Math.Sin(theta) / metersPerDegLon;
                points.Add((centerLat + dLat, centerLon + dLon));
            }

            return points;
        }

        /// <summary>
        /// Derives an equivalent circle radius (in metres) from a surface area in square metres.
        /// </summary>
        public static double RadiusFromAreaMeters(double areaSqM)
            => areaSqM <= 0 ? 0 : Math.Sqrt(areaSqM / Math.PI);

        /// <summary>
        /// Computes the centroid (mean vertex) of the first usable ring of a GeoJSON polygon /
        /// multipolygon. Returns null when no ring with at least three vertices can be parsed.
        /// </summary>
        public static (double Latitude, double Longitude)? PolygonCentroid(string? polygonGeoJson)
        {
            var ring = ParsePolygon(polygonGeoJson);
            return RingCentroid(ring);
        }

        /// <summary>
        /// Computes the centroid (mean vertex) of a ring of (latitude, longitude) points. Returns
        /// null when the ring has fewer than three vertices or contains only non-finite values.
        /// </summary>
        public static (double Latitude, double Longitude)? RingCentroid(
            IReadOnlyList<(double Latitude, double Longitude)> ring)
        {
            if (ring is null || ring.Count < 3)
            {
                return null;
            }

            double sumLat = 0, sumLon = 0;
            var count = 0;
            foreach (var (lat, lon) in ring)
            {
                if (double.IsNaN(lat) || double.IsNaN(lon)
                    || double.IsInfinity(lat) || double.IsInfinity(lon))
                {
                    continue;
                }

                sumLat += lat;
                sumLon += lon;
                count++;
            }

            return count == 0 ? null : (sumLat / count, sumLon / count);
        }

        /// <summary>
        /// Returns the coastline (land) polygons used for shoreline-impact detection, expressed as
        /// rings of (latitude, longitude) points. The default implementation supplies a coarse
        /// Algoa Bay shoreline; replace <see cref="LoadLandPolygonsFromGeoJson"/> output with a
        /// higher-resolution coastline if one becomes available.
        /// </summary>
        public static List<List<(double Latitude, double Longitude)>> LoadLandPolygons()
        {
            // Algoa Bay landmass approximation. The bay opens to the east/south-east, so land lies
            // to the WEST and NORTH of the coastline. The ring below traces the western/northern
            // shoreline (south -> north) and then closes far inland to the west, so the enclosed
            // area represents land. Any slick drifting onto the shore intersects this polygon.
            // Coordinates are (latitude, longitude).
            var algoaBayLand = new List<(double Latitude, double Longitude)>
            {
                // --- Coastline, south to north (water-facing edge) ---
                (-34.0500, 25.7050), // Cape Recife (southern tip)
                (-34.0000, 25.6700), // Schoenmakerskop / Humewood
                (-33.9700, 25.6300), // Port Elizabeth harbour / waterfront
                (-33.9300, 25.6100), // Summerstrand / north of harbour
                (-33.8900, 25.6000),
                (-33.8500, 25.6000), // Swartkops / Bluewater Bay
                (-33.8100, 25.6300),
                (-33.7800, 25.6900), // Coega / Ngqura
                (-33.7400, 25.8000),
                (-33.7100, 25.9200), // Sundays River mouth (north-east shore)
                // --- Close the ring inland (far west) so the enclosed area is land ---
                (-33.7100, 25.1000),
                (-34.0500, 25.1000),
                (-34.0500, 25.7050)  // back to start
            };

            return new List<List<(double Latitude, double Longitude)>>
            {
                algoaBayLand
            };
        }

        /// <summary>
        /// Parses a GeoJSON string (Polygon, MultiPolygon, or a FeatureCollection of those) into a
        /// list of (latitude, longitude) rings suitable for land-polygon intersection tests.
        /// </summary>
        public static List<List<(double Latitude, double Longitude)>> LoadLandPolygonsFromGeoJson(string? geoJson)
        {
            var polygons = new List<List<(double Latitude, double Longitude)>>();
            if (string.IsNullOrWhiteSpace(geoJson))
            {
                return polygons;
            }

            try
            {
                using var document = JsonDocument.Parse(geoJson);
                CollectPolygonRings(document.RootElement, polygons);
            }
            catch (JsonException)
            {
                // Malformed GeoJSON - return whatever was parsed.
            }

            return polygons;
        }

        private static void CollectPolygonRings(
            JsonElement element, List<List<(double Latitude, double Longitude)>> polygons)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            // FeatureCollection -> iterate features.
            if (element.TryGetProperty("features", out var features) &&
                features.ValueKind == JsonValueKind.Array)
            {
                foreach (var feature in features.EnumerateArray())
                {
                    CollectPolygonRings(feature, polygons);
                }
                return;
            }

            // Feature -> descend into geometry.
            var geometry = element;
            if (element.TryGetProperty("geometry", out var geom))
            {
                geometry = geom;
            }

            if (!geometry.TryGetProperty("type", out var typeElement) ||
                !geometry.TryGetProperty("coordinates", out var coordinates))
            {
                return;
            }

            var type = typeElement.GetString();
            if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
            {
                AddRing(coordinates[0], polygons);
            }
            else if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var polygon in coordinates.EnumerateArray())
                {
                    AddRing(polygon[0], polygons);
                }
            }
        }

        private static void AddRing(
            JsonElement ring, List<List<(double Latitude, double Longitude)>> polygons)
        {
            var points = new List<(double Latitude, double Longitude)>();
            foreach (var pair in ring.EnumerateArray())
            {
                // GeoJSON stores [longitude, latitude].
                var lon = pair[0].GetDouble();
                var lat = pair[1].GetDouble();
                points.Add((lat, lon));
            }

            if (points.Count >= 3)
            {
                polygons.Add(points);
            }
        }

        /// <summary>
        /// Returns true if two polygons (rings of lat/lon points) intersect. Uses a bounding-box
        /// precheck, an edge-crossing test, and a point-in-polygon containment test so that fully
        /// enclosed polygons are also detected.
        /// </summary>
        public static bool PolygonIntersectsPolygon(
            IReadOnlyList<(double Latitude, double Longitude)> polyA,
            IReadOnlyList<(double Latitude, double Longitude)> polyB)
        {
            if (polyA is null || polyB is null || polyA.Count < 3 || polyB.Count < 3)
            {
                return false;
            }

            // Cheap bounding-box rejection first.
            if (!BoundingBoxesOverlap(polyA, polyB))
            {
                return false;
            }

            // Any edge crossing means the boundaries intersect.
            for (var i = 0; i < polyA.Count; i++)
            {
                var a1 = polyA[i];
                var a2 = polyA[(i + 1) % polyA.Count];

                for (var j = 0; j < polyB.Count; j++)
                {
                    var b1 = polyB[j];
                    var b2 = polyB[(j + 1) % polyB.Count];

                    if (SegmentsIntersect(a1, a2, b1, b2))
                    {
                        return true;
                    }
                }
            }

            // No edge crossing: one polygon may be fully contained in the other.
            return PointInPolygon(polyA[0], polyB) || PointInPolygon(polyB[0], polyA);
        }

        private static bool BoundingBoxesOverlap(
            IReadOnlyList<(double Latitude, double Longitude)> a,
            IReadOnlyList<(double Latitude, double Longitude)> b)
        {
            double aMinLat = double.MaxValue, aMaxLat = double.MinValue;
            double aMinLon = double.MaxValue, aMaxLon = double.MinValue;
            foreach (var (lat, lon) in a)
            {
                aMinLat = Math.Min(aMinLat, lat);
                aMaxLat = Math.Max(aMaxLat, lat);
                aMinLon = Math.Min(aMinLon, lon);
                aMaxLon = Math.Max(aMaxLon, lon);
            }

            double bMinLat = double.MaxValue, bMaxLat = double.MinValue;
            double bMinLon = double.MaxValue, bMaxLon = double.MinValue;
            foreach (var (lat, lon) in b)
            {
                bMinLat = Math.Min(bMinLat, lat);
                bMaxLat = Math.Max(bMaxLat, lat);
                bMinLon = Math.Min(bMinLon, lon);
                bMaxLon = Math.Max(bMaxLon, lon);
            }

            return aMinLat <= bMaxLat && aMaxLat >= bMinLat
                && aMinLon <= bMaxLon && aMaxLon >= bMinLon;
        }

        private static bool SegmentsIntersect(
            (double Latitude, double Longitude) p1,
            (double Latitude, double Longitude) p2,
            (double Latitude, double Longitude) p3,
            (double Latitude, double Longitude) p4)
        {
            // Treat longitude as x and latitude as y for the planar test.
            var d1 = Cross(p3, p4, p1);
            var d2 = Cross(p3, p4, p2);
            var d3 = Cross(p1, p2, p3);
            var d4 = Cross(p1, p2, p4);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }

            // Collinear/touching cases.
            if (d1 == 0 && OnSegment(p3, p4, p1)) return true;
            if (d2 == 0 && OnSegment(p3, p4, p2)) return true;
            if (d3 == 0 && OnSegment(p1, p2, p3)) return true;
            if (d4 == 0 && OnSegment(p1, p2, p4)) return true;

            return false;
        }

        private static double Cross(
            (double Latitude, double Longitude) a,
            (double Latitude, double Longitude) b,
            (double Latitude, double Longitude) c)
        {
            // Cross product of (b-a) x (c-a) using lon=x, lat=y.
            return ((b.Longitude - a.Longitude) * (c.Latitude - a.Latitude))
                 - ((b.Latitude - a.Latitude) * (c.Longitude - a.Longitude));
        }

        private static bool OnSegment(
            (double Latitude, double Longitude) a,
            (double Latitude, double Longitude) b,
            (double Latitude, double Longitude) p)
        {
            return Math.Min(a.Longitude, b.Longitude) <= p.Longitude
                && p.Longitude <= Math.Max(a.Longitude, b.Longitude)
                && Math.Min(a.Latitude, b.Latitude) <= p.Latitude
                && p.Latitude <= Math.Max(a.Latitude, b.Latitude);
        }

        /// <summary>
        /// Winding/ray-cast point-in-polygon test (lon=x, lat=y).
        /// </summary>
        private static bool PointInPolygon(
            (double Latitude, double Longitude) point,
            IReadOnlyList<(double Latitude, double Longitude)> polygon)
        {
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var yi = polygon[i].Latitude;
                var xi = polygon[i].Longitude;
                var yj = polygon[j].Latitude;
                var xj = polygon[j].Longitude;

                var intersects = ((yi > point.Latitude) != (yj > point.Latitude)) &&
                    (point.Longitude < (xj - xi) * (point.Latitude - yi) / (yj - yi + double.Epsilon) + xi);

                if (intersects)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static double DegToRad(double degrees) => degrees * Math.PI / 180.0;
    }
}
