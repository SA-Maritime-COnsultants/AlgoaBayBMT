using System.Text.Json;
using AlgoaBayBMT.Data;
using AlgoaBayBMT.Emergency.OilSpill.DTOs;
using AlgoaBayBMT.Emergency.OilSpill.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    /// <summary>
    /// Manages IMS/ICS incident-management forms and their auto-linking to the SITREP export.
    /// Linking rules:
    ///  - ICS-209 (Status Summary) is ALWAYS linked to the SITREP.
    ///  - ICS-201 (Briefing) is linked while the incident is new (Active and no model runs yet).
    ///  - ICS-214 (Activity Log) is linked once response actions exist.
    /// </summary>
    public sealed class IncidentFormService : IIncidentFormService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        /// <summary>Default operational period used when the caller does not specify one.</summary>
        public const int DefaultOperationalPeriodId = 1;

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IncidentFormService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IReadOnlyList<IncidentForm>> GetFormsForIncidentAsync(int incidentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.IncidentForms
                .Where(f => f.IncidentId == incidentId)
                .OrderByDescending(f => f.LastUpdatedAt ?? f.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IncidentForm?> GetFormByIdAsync(int formId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.IncidentForms.AsNoTracking().FirstOrDefaultAsync(f => f.Id == formId);
        }

        public async Task<IncidentForm?> GetFormByTypeAsync(int incidentId, IncidentFormType formType)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.IncidentForms
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.IncidentId == incidentId && f.FormType == formType);
        }

        public async Task<IReadOnlyList<IncidentForm>> GetFormsByTypeAsync(int incidentId, IncidentFormType formType)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.IncidentForms
                .Where(f => f.IncidentId == incidentId && f.FormType == formType)
                .OrderByDescending(f => f.LastUpdatedAt ?? f.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IncidentForm> CreateFormAsync(
            int incidentId,
            IncidentFormType formType,
            string jsonData,
            string createdBy,
            int? operationalPeriodId = null,
            string? personName = null,
            bool linkToSitrep = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            var form = new IncidentForm
            {
                IncidentId = incidentId,
                FormType = formType,
                JsonData = string.IsNullOrWhiteSpace(jsonData) ? "{}" : jsonData,
                Status = IncidentFormStatus.Draft,
                IsLinkedToSitrep = linkToSitrep,
                OperationalPeriodId = operationalPeriodId,
                PersonName = personName,
                CreatedAt = now,
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "System" : createdBy,
                LastUpdatedAt = now
            };

            context.IncidentForms.Add(form);
            await context.SaveChangesAsync();
            return form;
        }

        public async Task<bool> UpdateFormAsync(int formId, string jsonData, IncidentFormStatus? status = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var form = await context.IncidentForms.FirstOrDefaultAsync(f => f.Id == formId);
            if (form is null)
            {
                return false;
            }

            form.JsonData = string.IsNullOrWhiteSpace(jsonData) ? "{}" : jsonData;
            if (status.HasValue)
            {
                form.Status = status.Value;
            }
            form.LastUpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFormAsync(int formId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var form = await context.IncidentForms.FirstOrDefaultAsync(f => f.Id == formId);
            if (form is null)
            {
                return false;
            }

            context.IncidentForms.Remove(form);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IncidentForm> AddActivityLogEntryAsync(
            int incidentId,
            string personName,
            string activity,
            string? notes,
            int operationalPeriodId,
            string createdBy)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var incident = await context.OilSpillIncidents.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == incidentId);

            var now = DateTime.UtcNow;
            var payload = new Ics214EntryPayload
            {
                IncidentName = incident?.SpillName ?? string.Empty,
                Timestamp = now,
                PersonName = personName,
                Activity = activity,
                Notes = notes ?? string.Empty
            };

            var form = new IncidentForm
            {
                IncidentId = incidentId,
                FormType = IncidentFormType.ICS214,
                JsonData = Serialize(payload),
                Status = IncidentFormStatus.Draft,
                IsLinkedToSitrep = true, // ICS-214 entries feed the consolidated SITREP log.
                OperationalPeriodId = operationalPeriodId,
                PersonName = personName,
                CreatedAt = now,
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? personName : createdBy,
                LastUpdatedAt = now
            };

            context.IncidentForms.Add(form);
            await context.SaveChangesAsync();
            return form;
        }

        public async Task<IncidentForm> EnsureAssignmentListAsync(
            int incidentId,
            OilSpillActionType actionType,
            string division,
            int operationalPeriodId,
            string createdBy)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var incident = await context.OilSpillIncidents.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == incidentId);

            var now = DateTime.UtcNow;
            var payload = new Ics204Payload
            {
                IncidentName = incident?.SpillName ?? string.Empty,
                Division = string.IsNullOrWhiteSpace(division) ? actionType.ToString() : division,
                BranchOrGroup = "Operations",
                OperationsLeader = incident?.Commander ?? string.Empty,
                OperationalPeriodStart = now,
                Resources = $"Auto-generated for {actionType} deployment.",
                Assignment = $"Execute {actionType} response measure and report progress via ICS-214.",
                SpecialInstructions = "Maintain responder safety and coordinate with the Operations Section Chief."
            };

            var form = new IncidentForm
            {
                IncidentId = incidentId,
                FormType = IncidentFormType.ICS204,
                JsonData = Serialize(payload),
                Status = IncidentFormStatus.Draft,
                IsLinkedToSitrep = false,
                OperationalPeriodId = operationalPeriodId,
                CreatedAt = now,
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Operations" : createdBy,
                LastUpdatedAt = now
            };

            context.IncidentForms.Add(form);
            await context.SaveChangesAsync();
            return form;
        }

        public async Task EnsureInitialFormsAsync(int incidentId, string createdBy)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var incident = await context.OilSpillIncidents.FirstOrDefaultAsync(s => s.Id == incidentId);
            if (incident is null)
            {
                return;
            }

            var existingTypes = await context.IncidentForms
                .Where(f => f.IncidentId == incidentId)
                .Select(f => f.FormType)
                .ToListAsync();

            var now = DateTime.UtcNow;

            // ICS-209 - always created and always linked to the SITREP.
            if (!existingTypes.Contains(IncidentFormType.ICS209))
            {
                var payload = BuildStatusSummary(incident, null, null);
                context.IncidentForms.Add(new IncidentForm
                {
                    IncidentId = incidentId,
                    FormType = IncidentFormType.ICS209,
                    JsonData = Serialize(payload),
                    Status = IncidentFormStatus.Draft,
                    IsLinkedToSitrep = true,
                    CreatedAt = now,
                    CreatedBy = createdBy,
                    LastUpdatedAt = now
                });
            }

            // ICS-201 - briefing for a new incident; linked while the incident is new.
            if (!existingTypes.Contains(IncidentFormType.ICS201))
            {
                var briefing = new Ics201Payload
                {
                    IncidentName = incident.SpillName,
                    Commander = incident.Commander ?? string.Empty,
                    IncidentStartTime = incident.SpillStartTime,
                    SourceType = incident.SourceType.ToString(),
                    ProductType = incident.ProductType.ToString(),
                    EstimatedVolume = incident.EstimatedVolume,
                    CurrentSituation = $"Oil spill reported at {incident.Latitude:F4}, {incident.Longitude:F4}.",
                    InitialObjectives = "Ensure responder safety, contain the release, and protect sensitive shoreline.",
                    SafetyMessage = "Maintain safe approach distances and monitor air quality near the slick."
                };

                context.IncidentForms.Add(new IncidentForm
                {
                    IncidentId = incidentId,
                    FormType = IncidentFormType.ICS201,
                    JsonData = Serialize(briefing),
                    Status = IncidentFormStatus.Draft,
                    IsLinkedToSitrep = incident.Status == OilSpillStatus.Active,
                    CreatedAt = now,
                    CreatedBy = createdBy,
                    LastUpdatedAt = now
                });
            }

            await context.SaveChangesAsync();
        }

        public async Task<IncidentForm?> SyncStatusSummaryAsync(int incidentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var incident = await context.OilSpillIncidents.FirstOrDefaultAsync(s => s.Id == incidentId);
            if (incident is null)
            {
                return null;
            }

            var latestRun = await context.OilSpillModelRuns
                .Where(r => r.SpillId == incidentId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            IReadOnlyList<OilSpillTrajectoryPoint> trajectory = latestRun is null
                ? Array.Empty<OilSpillTrajectoryPoint>()
                : await context.OilSpillTrajectoryPoints
                    .Where(p => p.ModelRunId == latestRun.Id)
                    .OrderBy(p => p.Timestamp)
                    .AsNoTracking()
                    .ToListAsync();

            var responseCount = await context.OilSpillResponseActions
                .CountAsync(a => a.SpillId == incidentId);

            var payload = BuildStatusSummary(incident, latestRun, trajectory);
            payload.ResponseMeasureCount = responseCount;

            var form = await context.IncidentForms
                .FirstOrDefaultAsync(f => f.IncidentId == incidentId && f.FormType == IncidentFormType.ICS209);

            var now = DateTime.UtcNow;
            if (form is null)
            {
                form = new IncidentForm
                {
                    IncidentId = incidentId,
                    FormType = IncidentFormType.ICS209,
                    Status = IncidentFormStatus.Draft,
                    IsLinkedToSitrep = true,
                    CreatedAt = now,
                    CreatedBy = incident.CreatedBy,
                };
                context.IncidentForms.Add(form);
            }

            form.JsonData = Serialize(payload);
            form.IsLinkedToSitrep = true; // ICS-209 must always be linked.
            form.LastUpdatedAt = now;

            await context.SaveChangesAsync();
            return form;
        }

        public async Task LogActivityAsync(int incidentId, string activity, string performedBy)
        {
            // Each activity is now persisted as its own ICS-214 entry record (one entry == one
            // form) under the default operational period so multiple contributors log independently.
            var person = string.IsNullOrWhiteSpace(performedBy) ? "Response Team" : performedBy;
            await AddActivityLogEntryAsync(
                incidentId,
                person,
                activity,
                notes: null,
                operationalPeriodId: DefaultOperationalPeriodId,
                createdBy: person);
        }

        public async Task<IReadOnlyList<IncidentForm>> GetSitrepLinkedFormsAsync(int incidentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.IncidentForms
                .Where(f => f.IncidentId == incidentId && f.IsLinkedToSitrep)
                .OrderBy(f => f.FormType)
                .AsNoTracking()
                .ToListAsync();
        }

        private static Ics209Payload BuildStatusSummary(
            OilSpillIncident incident,
            OilSpillModelRun? run,
            IReadOnlyList<OilSpillTrajectoryPoint>? trajectory)
        {
            double maxAreaSqKm = 0;
            double driftKm = 0;

            if (trajectory is { Count: > 0 })
            {
                maxAreaSqKm = trajectory.Max(p => p.AreaSqM) / 1_000_000.0;
                var first = trajectory[0];
                var last = trajectory[^1];
                driftKm = HaversineKm(first.Latitude, first.Longitude, last.Latitude, last.Longitude);
            }

            var hasImpact = run?.ShorelineImpactTime is not null;

            return new Ics209Payload
            {
                IncidentName = incident.SpillName,
                Commander = incident.Commander ?? string.Empty,
                ProductType = incident.ProductType.ToString(),
                Status = incident.Status.ToString(),
                EstimatedVolume = incident.EstimatedVolume,
                Latitude = incident.Latitude,
                Longitude = incident.Longitude,
                ReportTime = DateTime.UtcNow,
                SituationSummary = hasImpact
                    ? "Modelled trajectory predicts shoreline contact; prioritise shoreline protection and recovery."
                    : "Slick remains offshore under modelled conditions; maintain containment and recovery.",
                MaxSlickAreaSqKm = maxAreaSqKm,
                TotalDriftDistanceKm = driftKm,
                HasShorelineImpact = hasImpact,
                ShorelineImpactTime = run?.ShorelineImpactTime,
                PlannedActions = "Continue monitoring, sustain containment, and update SITREP as conditions evolve."
            };
        }

        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double r = 6371.0;
            double dLat = DegToRad(lat2 - lat1);
            double dLon = DegToRad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double DegToRad(double deg) => deg * Math.PI / 180.0;

        private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

        private static T? Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }
    }
}
