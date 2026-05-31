using AlgoaBayBMT.Data;
using AlgoaBayBMT.Emergency.OilSpill.DTOs;
using AlgoaBayBMT.Emergency.OilSpill.Models;
using AlgoaBayBMT.Emergency.OilSpill.Visualization;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    public class OilSpillResponseService : IOilSpillResponseService
    {
        // Sensible default footprints when only a single coordinate is supplied.
        private const double DefaultBoomLengthMeters = 300.0;
        private const double DefaultSkimmerRadiusMeters = 150.0;
        private const double DefaultDispersantRadiusMeters = 250.0;

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IIncidentFormService _formService;

        public OilSpillResponseService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IIncidentFormService formService)
        {
            _contextFactory = contextFactory;
            _formService = formService;
        }

        public async Task<OilSpillResponseAction> AddResponseActionAsync(CreateOilSpillResponseActionRequest request)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var action = new OilSpillResponseAction
            {
                SpillId = request.SpillId,
                ActionType = request.ActionType,
                Description = request.Description,
                StartTime = request.StartTime,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                PerformedBy = request.PerformedBy,
                Notes = request.Notes
            };

            PopulateGeometry(action, request);

            context.OilSpillResponseActions.Add(action);
            await context.SaveChangesAsync();

            // Automation: log the response action to the ICS-214 activity log (auto-linked to SITREP).
            await _formService.LogActivityAsync(
                request.SpillId,
                $"{action.ActionType}: {action.Description}",
                string.IsNullOrWhiteSpace(action.PerformedBy) ? "Response Team" : action.PerformedBy);

            return action;
        }

        /// <summary>
        /// Generates <see cref="OilSpillResponseAction.GeometryGeoJson"/> and
        /// <see cref="OilSpillResponseAction.RadiusMeters"/> from the action type and the supplied
        /// coordinate. When no coordinate is available the geometry is left null.
        /// </summary>
        private static void PopulateGeometry(
            OilSpillResponseAction action, CreateOilSpillResponseActionRequest request)
        {
            if (action.Latitude is not double lat || action.Longitude is not double lon)
            {
                return;
            }

            switch (action.ActionType)
            {
                case OilSpillActionType.DeployBoom:
                    // Default boom: a short line segment oriented across the bay mouth.
                    action.GeometryGeoJson = ResponseGeometryBuilder.BuildShortBoomGeoJson(
                        lat, lon, orientationDeg: 35.0, lengthMeters: DefaultBoomLengthMeters);
                    break;

                case OilSpillActionType.Skimmer:
                    action.RadiusMeters = request.RadiusMeters ?? DefaultSkimmerRadiusMeters;
                    action.GeometryGeoJson = ResponseGeometryBuilder.BuildPointGeoJson(lat, lon);
                    break;

                case OilSpillActionType.Dispersant:
                    action.RadiusMeters = request.RadiusMeters ?? DefaultDispersantRadiusMeters;
                    action.GeometryGeoJson = ResponseGeometryBuilder.BuildCirclePolygonGeoJson(
                        lat, lon, action.RadiusMeters.Value);
                    break;

                case OilSpillActionType.ShorelineProtection:
                    action.GeometryGeoJson = ResponseGeometryBuilder.BuildCoastalSegmentGeoJson(lat, lon);
                    break;

                case OilSpillActionType.AerialRecon:
                default:
                    // Future extension: a real flight path LineString. For now plot as a marker.
                    action.GeometryGeoJson = ResponseGeometryBuilder.BuildPointGeoJson(lat, lon);
                    break;
            }
        }

        public async Task<IReadOnlyList<OilSpillResponseAction>> GetActionsForSpillAsync(int spillId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.OilSpillResponseActions
                .Where(a => a.SpillId == spillId)
                .OrderByDescending(a => a.StartTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> CloseActionAsync(int actionId, DateTime endTime)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var action = await context.OilSpillResponseActions.FirstOrDefaultAsync(a => a.Id == actionId);
            if (action is null)
            {
                return false;
            }

            action.EndTime = endTime;

            await context.SaveChangesAsync();
            return true;
        }
    }
}
