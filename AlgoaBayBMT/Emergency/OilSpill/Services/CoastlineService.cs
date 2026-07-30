using AlgoaBayBMT.Emergency.OilSpill.Visualization;
using Microsoft.Extensions.Hosting;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    /// <summary>
    /// Provides high-resolution coastline (land) geometry and NetTopologySuite-backed spatial
    /// predicates used for shoreline-impact detection and slick-footprint unioning.
    /// </summary>
    public interface ICoastlineService
    {
        /// <summary>True when a usable coastline geometry was loaded.</summary>
        bool HasCoastline { get; }

        /// <summary>
        /// Returns true if the supplied GeoJSON polygon intersects the land geometry (i.e. the
        /// slick has reached the shoreline).
        /// </summary>
        bool IntersectsLand(string? polygonGeoJson);

        /// <summary>
        /// Returns true if the supplied ring (lat/lon points) intersects the land geometry.
        /// </summary>
        bool IntersectsLand(IReadOnlyList<(double Latitude, double Longitude)> ring);

        /// <summary>
        /// Unions two GeoJSON polygons and returns the resulting footprint as a GeoJSON polygon
        /// (or multipolygon) string. When <paramref name="cumulativeGeoJson"/> is null/empty the
        /// instantaneous polygon is returned unchanged.
        /// </summary>
        string? Union(string? cumulativeGeoJson, string? instantaneousGeoJson);

        /// <summary>
        /// Clips the supplied GeoJSON polygon so it only covers water (subtracts the land
        /// geometry). The visible edge therefore follows the coastline once the slick reaches it.
        /// Returns the original polygon when no coastline is loaded or the clip fails.
        /// </summary>
        string? ClipToWater(string? polygonGeoJson);

        /// <summary>
        /// Returns the representative geographic point where the supplied slick polygon overlaps
        /// land (i.e. the shoreline-impact location), or null when the slick does not reach the
        /// coast or no coastline is loaded.
        /// </summary>
        (double Latitude, double Longitude)? LandfallPoint(string? polygonGeoJson);

        /// <summary>
        /// Returns the stretch of coastline touched by the supplied slick footprint as GeoJSON
        /// line geometry (LineString/MultiLineString), or null when the slick does not reach the
        /// coast. This is the "potential shoreline impact" zone drawn along the shore.
        /// </summary>
        string? ImpactZone(string? slickGeoJson);
    }

    public sealed class CoastlineService : ICoastlineService
    {
        private readonly Geometry? _land;
        private readonly GeoJsonReader _reader = new();
        private readonly GeoJsonWriter _writer = new();

        public CoastlineService(IHostEnvironment environment)
        {
            _land = LoadLand(environment);
        }

        public bool HasCoastline => _land is not null && !_land.IsEmpty;

        public bool IntersectsLand(string? polygonGeoJson)
        {
            if (_land is null || string.IsNullOrWhiteSpace(polygonGeoJson))
            {
                return false;
            }

            try
            {
                var geometry = _reader.Read<Geometry>(polygonGeoJson);
                return geometry is not null && !geometry.IsEmpty && _land.Intersects(geometry);
            }
            catch
            {
                return false;
            }
        }

        public bool IntersectsLand(IReadOnlyList<(double Latitude, double Longitude)> ring)
        {
            if (_land is null || ring is null || ring.Count < 3)
            {
                return false;
            }

            var polygon = ToPolygon(ring);
            return polygon is not null && _land.Intersects(polygon);
        }

        public string? Union(string? cumulativeGeoJson, string? instantaneousGeoJson)
        {
            if (string.IsNullOrWhiteSpace(instantaneousGeoJson))
            {
                return cumulativeGeoJson;
            }

            if (string.IsNullOrWhiteSpace(cumulativeGeoJson))
            {
                return instantaneousGeoJson;
            }

            try
            {
                var current = _reader.Read<Geometry>(cumulativeGeoJson);
                var next = _reader.Read<Geometry>(instantaneousGeoJson);
                if (current is null || current.IsEmpty)
                {
                    return instantaneousGeoJson;
                }
                if (next is null || next.IsEmpty)
                {
                    return cumulativeGeoJson;
                }

                var union = current.Union(next);
                return _writer.Write(union);
            }
            catch
            {
                // Fall back to the latest footprint if the union fails for any reason.
                return instantaneousGeoJson;
            }
        }

        public string? ClipToWater(string? polygonGeoJson)
        {
            if (_land is null || _land.IsEmpty || string.IsNullOrWhiteSpace(polygonGeoJson))
            {
                return polygonGeoJson;
            }

            try
            {
                var slick = _reader.Read<Geometry>(polygonGeoJson);
                if (slick is null || slick.IsEmpty)
                {
                    return polygonGeoJson;
                }

                // No interaction with land -> return the slick unchanged (cheap fast-path).
                if (!_land.Intersects(slick))
                {
                    return polygonGeoJson;
                }

                // Subtract the land so the slick only covers water; its edge now traces the coast.
                var water = slick.Difference(_land);
                if (water is null || water.IsEmpty)
                {
                    return polygonGeoJson;
                }

                return _writer.Write(water);
            }
            catch
            {
                return polygonGeoJson;
            }
        }

        public (double Latitude, double Longitude)? LandfallPoint(string? polygonGeoJson)
        {
            if (_land is null || _land.IsEmpty || string.IsNullOrWhiteSpace(polygonGeoJson))
            {
                return null;
            }

            try
            {
                var slick = _reader.Read<Geometry>(polygonGeoJson);
                if (slick is null || slick.IsEmpty || !_land.Intersects(slick))
                {
                    return null;
                }

                // The part of the slick lying over land is the impacted area; its interior point
                // is a stable, guaranteed-on-geometry estimate of the shoreline-impact location.
                var overlap = _land.Intersection(slick);
                if (overlap is null || overlap.IsEmpty)
                {
                    return null;
                }

                var point = overlap.InteriorPoint;
                if (point is null || point.IsEmpty)
                {
                    return null;
                }

                // NTS coordinates are (X = longitude, Y = latitude).
                return (point.Y, point.X);
            }
            catch
            {
                return null;
            }
        }

        public string? ImpactZone(string? slickGeoJson)
        {
            if (_land is null || _land.IsEmpty || string.IsNullOrWhiteSpace(slickGeoJson))
            {
                return null;
            }

            try
            {
                var slick = _reader.Read<Geometry>(slickGeoJson);
                if (slick is null || slick.IsEmpty)
                {
                    return null;
                }

                // Buffer the slick slightly (~50 m in degrees) so a footprint clipped exactly to
                // the waterline still registers the coastline segment it presses against.
                var probe = slick.Buffer(0.0005);
                var boundary = _land.Boundary;
                if (boundary is null || boundary.IsEmpty || !boundary.Intersects(probe))
                {
                    return null;
                }

                var zone = boundary.Intersection(probe);
                if (zone is null || zone.IsEmpty)
                {
                    return null;
                }

                return _writer.Write(zone);
            }
            catch
            {
                return null;
            }
        }

        private static Polygon? ToPolygon(IReadOnlyList<(double Latitude, double Longitude)> ring)
        {
            // NTS uses (X = longitude, Y = latitude). Ensure the ring is closed.
            var coords = new List<Coordinate>(ring.Count + 1);
            foreach (var (lat, lon) in ring)
            {
                coords.Add(new Coordinate(lon, lat));
            }

            if (coords.Count < 3)
            {
                return null;
            }

            if (!coords[0].Equals2D(coords[^1]))
            {
                coords.Add(new Coordinate(coords[0].X, coords[0].Y));
            }

            try
            {
                var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();
                return factory.CreatePolygon(coords.ToArray());
            }
            catch
            {
                return null;
            }
        }

        private static Geometry? LoadLand(IHostEnvironment environment)
        {
            try
            {
                var path = Path.Combine(
                    environment.ContentRootPath, "wwwroot", "data", "algoabay-coastline.geojson");

                if (File.Exists(path))
                {
                    var geoJson = File.ReadAllText(path);
                    var land = ParseFeatureCollection(geoJson);
                    if (land is not null && !land.IsEmpty)
                    {
                        return land;
                    }
                }
            }
            catch
            {
                // Fall through to the built-in approximation below.
            }

            // Fall back to the coarse built-in Algoa Bay land polygon.
            return BuildFallbackLand();
        }

        private static Geometry? ParseFeatureCollection(string geoJson)
        {
            var rings = OilSpillGeometry.LoadLandPolygonsFromGeoJson(geoJson);
            return BuildFromRings(rings);
        }

        private static Geometry? BuildFallbackLand()
        {
            var rings = OilSpillGeometry.LoadLandPolygons();
            return BuildFromRings(rings);
        }

        private static Geometry? BuildFromRings(
            IReadOnlyList<List<(double Latitude, double Longitude)>> rings)
        {
            if (rings is null || rings.Count == 0)
            {
                return null;
            }

            var polygons = new List<Geometry>();
            foreach (var ring in rings)
            {
                var polygon = ToPolygon(ring);
                if (polygon is not null && polygon.IsValid)
                {
                    polygons.Add(polygon);
                }
                else if (polygon is not null)
                {
                    // Repair self-intersections via a zero-width buffer.
                    var fixedGeom = polygon.Buffer(0);
                    if (!fixedGeom.IsEmpty)
                    {
                        polygons.Add(fixedGeom);
                    }
                }
            }

            if (polygons.Count == 0)
            {
                return null;
            }

            return polygons.Count == 1 ? polygons[0] : CascadedPolygonUnion.Union(polygons);
        }
    }
}
