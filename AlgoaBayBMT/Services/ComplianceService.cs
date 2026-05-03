using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class ComplianceService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<ComplianceService> logger) : IComplianceService
    {
        public async Task<List<ComplianceRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.ComplianceRules.AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Scope).ThenBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<ComplianceResult> EvaluateCrewMemberAsync(int crewMemberId, int? vesselId, CrewRank? rankContext, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var crew = await db.CrewMembers
                .Include(c => c.Documents.Where(d => !d.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == crewMemberId, cancellationToken)
                ?? throw new InvalidOperationException($"CrewMember {crewMemberId} not found.");

            Vessel? vessel = null;
            if (vesselId.HasValue)
            {
                vessel = await db.Vessels.FirstOrDefaultAsync(v => v.Id == vesselId.Value, cancellationToken);
            }

            var rank = rankContext ?? crew.Rank;
            var rules = await db.ComplianceRules.AsNoTracking().Where(r => r.IsActive).ToListAsync(cancellationToken);

            // Course completions for this crew member's linked user (if any)
            List<Guid> completedCourseIds = new();
            DateTime nowUtc = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(crew.ApplicationUserId))
            {
                completedCourseIds = await db.Set<CourseCompletionRecord>()
                    .Where(r => r.UserId == crew.ApplicationUserId)
                    .Select(r => r.CourseId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }

            var missingRuleIds = new List<int>();
            var missingCourseIds = new List<Guid>();
            var summary = new System.Text.StringBuilder();
            var state = ComplianceState.Valid;

            foreach (var rule in rules)
            {
                if (!RuleApplies(rule, rank, vessel, crew)) continue;

                var failed = false;

                if (rule.RequiredCourseId.HasValue)
                {
                    if (!completedCourseIds.Contains(rule.RequiredCourseId.Value))
                    {
                        failed = true;
                        missingCourseIds.Add(rule.RequiredCourseId.Value);
                        summary.AppendLine($"Missing required course for rule '{rule.Name}'.");
                    }
                }

                if (rule.RequiredDocumentType.HasValue)
                {
                    var doc = crew.Documents.FirstOrDefault(d => d.DocumentType == rule.RequiredDocumentType.Value);
                    if (doc is null)
                    {
                        failed = true;
                        summary.AppendLine($"Missing document {rule.RequiredDocumentType} for rule '{rule.Name}'.");
                    }
                    else if (doc.ExpiryDate.HasValue)
                    {
                        var daysLeft = (doc.ExpiryDate.Value - nowUtc).TotalDays;
                        if (daysLeft <= 0)
                        {
                            failed = true;
                            summary.AppendLine($"Document {rule.RequiredDocumentType} expired on {doc.ExpiryDate:yyyy-MM-dd}.");
                        }
                        else if (daysLeft <= 30 && state == ComplianceState.Valid)
                        {
                            state = ComplianceState.Expiring;
                            summary.AppendLine($"Document {rule.RequiredDocumentType} expiring in {(int)daysLeft} days.");
                        }
                    }
                }

                if (failed)
                {
                    missingRuleIds.Add(rule.Id);
                    state = ComplianceState.NonCompliant;
                }
            }

            var result = new ComplianceResult
            {
                CrewMemberId = crew.Id,
                VesselId = vessel?.Id,
                EvaluatedOnUtc = nowUtc,
                State = state,
                MissingRuleIds = missingRuleIds.Count > 0 ? string.Join(",", missingRuleIds) : null,
                AssignedCourseIds = missingCourseIds.Count > 0 ? string.Join(",", missingCourseIds) : null,
                Summary = summary.Length > 0 ? summary.ToString() : "All applicable rules satisfied.",
                EvaluatedByUserId = performedByUserId
            };

            db.ComplianceResults.Add(result);

            db.ComplianceAuditEntries.Add(new ComplianceAuditEntry
            {
                Action = vessel is null ? ComplianceAuditAction.VesselEvaluated : ComplianceAuditAction.CrewSignOnEvaluated,
                CrewMemberId = crew.Id,
                VesselId = vessel?.Id,
                Details = $"State={state}; MissingRules={result.MissingRuleIds}; MissingCourses={result.AssignedCourseIds}",
                OccurredOnUtc = nowUtc,
                PerformedByUserId = performedByUserId
            });

            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Compliance evaluated for crew {Id} -> {State}", crew.Id, state);
            return result;
        }

        public async Task<VesselComplianceSnapshot> EvaluateVesselAsync(int vesselId, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var vessel = await db.Vessels.FirstOrDefaultAsync(v => v.Id == vesselId, cancellationToken)
                         ?? throw new InvalidOperationException($"Vessel {vesselId} not found.");

            var activeAssignments = await db.CrewAssignments
                .Where(a => a.VesselId == vesselId && a.Status == CrewAssignmentStatus.SignedOn)
                .Select(a => new { a.CrewMemberId, a.RankOnAssignment })
                .ToListAsync(cancellationToken);

            int compliant = 0, nonCompliant = 0, expiring = 0;
            foreach (var a in activeAssignments)
            {
                var r = await EvaluateCrewMemberAsync(a.CrewMemberId, vesselId, a.RankOnAssignment, performedByUserId, cancellationToken);
                switch (r.State)
                {
                    case ComplianceState.Valid: compliant++; break;
                    case ComplianceState.Expiring: expiring++; break;
                    case ComplianceState.NonCompliant:
                    case ComplianceState.Expired: nonCompliant++; break;
                }
            }

            var snapshotState = nonCompliant > 0
                ? VesselComplianceState.NonCompliant
                : (expiring > 0 ? VesselComplianceState.PartiallyCompliant : VesselComplianceState.FullyCompliant);

            var snapshot = new VesselComplianceSnapshot
            {
                VesselId = vesselId,
                EvaluatedOnUtc = DateTime.UtcNow,
                State = snapshotState,
                CompliantCrewCount = compliant,
                NonCompliantCrewCount = nonCompliant,
                ExpiringSoonCount = expiring,
                TotalCrewCount = activeAssignments.Count,
                Summary = $"Active crew: {activeAssignments.Count}; Compliant: {compliant}; Expiring: {expiring}; Non-compliant: {nonCompliant}.",
                EvaluatedByUserId = performedByUserId
            };
            db.VesselComplianceSnapshots.Add(snapshot);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Vessel {VesselId} compliance: {State}", vesselId, snapshotState);
            return snapshot;
        }

        public async Task<int> ScanExpiriesAsync(int horizonDays = 60, CancellationToken cancellationToken = default)
        {
            if (horizonDays <= 0) horizonDays = 60;
            var nowUtc = DateTime.UtcNow;
            var horizonUtc = nowUtc.AddDays(horizonDays);
            int created = 0;

            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            // Documents expiring within the horizon
            var expiringDocs = await db.CrewDocuments.AsNoTracking()
                .Where(d => !d.IsDeleted && d.ExpiryDate != null && d.ExpiryDate <= horizonUtc)
                .Select(d => new { d.Id, d.CrewMemberId, d.ExpiryDate })
                .ToListAsync(cancellationToken);

            foreach (var d in expiringDocs)
            {
                bool exists = await db.CertificateExpiryEvents
                    .AnyAsync(e => e.CrewDocumentId == d.Id && e.Status != ExpiryEventStatus.Expired, cancellationToken);
                if (exists) continue;
                db.CertificateExpiryEvents.Add(new CertificateExpiryEvent
                {
                    CrewMemberId = d.CrewMemberId,
                    Source = ExpirySource.CrewDocument,
                    CrewDocumentId = d.Id,
                    ExpiryDateUtc = d.ExpiryDate!.Value,
                    DaysUntilExpiry = Math.Max(0, (int)(d.ExpiryDate.Value - nowUtc).TotalDays),
                    Status = d.ExpiryDate.Value <= nowUtc ? ExpiryEventStatus.Expired : ExpiryEventStatus.Pending,
                    DetectedOnUtc = nowUtc
                });
                created++;
            }

            // Training certificates expiring within the horizon
            var expiringCerts = await (
                from cert in db.Set<TrainingCertificate>().AsNoTracking()
                join ccr in db.Set<CourseCompletionRecord>().AsNoTracking()
                    on cert.CourseCompletionRecordId equals ccr.CourseCompletionRecordId
                join crew in db.CrewMembers.AsNoTracking()
                    on ccr.UserId equals crew.ApplicationUserId
                where cert.ExpiresOnUtc <= horizonUtc && cert.RevokedOnUtc == null
                select new { cert.TrainingCertificateId, CrewId = crew.Id, cert.ExpiresOnUtc }
            ).ToListAsync(cancellationToken);

            foreach (var c in expiringCerts)
            {
                bool exists = await db.CertificateExpiryEvents
                    .AnyAsync(e => e.TrainingCertificateId == c.TrainingCertificateId && e.Status != ExpiryEventStatus.Expired, cancellationToken);
                if (exists) continue;
                db.CertificateExpiryEvents.Add(new CertificateExpiryEvent
                {
                    CrewMemberId = c.CrewId,
                    Source = ExpirySource.TrainingCertificate,
                    TrainingCertificateId = c.TrainingCertificateId,
                    ExpiryDateUtc = c.ExpiresOnUtc,
                    DaysUntilExpiry = Math.Max(0, (int)(c.ExpiresOnUtc - nowUtc).TotalDays),
                    Status = c.ExpiresOnUtc <= nowUtc ? ExpiryEventStatus.Expired : ExpiryEventStatus.Pending,
                    DetectedOnUtc = nowUtc
                });
                created++;
            }

            if (created > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("ScanExpiriesAsync created {Count} expiry events", created);
            }
            return created;
        }

        private static bool RuleApplies(ComplianceRule rule, CrewRank? rank, Vessel? vessel, CrewMember crew)
        {
            return rule.Scope switch
            {
                ComplianceRequirementScope.Universal => true,
                ComplianceRequirementScope.Rank => rule.RequiredForRank.HasValue && rank.HasValue && rule.RequiredForRank == rank,
                ComplianceRequirementScope.VesselType => vessel is not null
                    && !string.IsNullOrEmpty(rule.RequiredForVesselType)
                    && string.Equals(vessel.VesselType, rule.RequiredForVesselType, StringComparison.OrdinalIgnoreCase),
                ComplianceRequirementScope.Operator => rule.RequiredForOperatorId.HasValue
                    && (vessel?.OwningOperatorId == rule.RequiredForOperatorId
                        || crew.EmployerOperatorId == rule.RequiredForOperatorId),
                _ => false
            };
        }
    }
}
