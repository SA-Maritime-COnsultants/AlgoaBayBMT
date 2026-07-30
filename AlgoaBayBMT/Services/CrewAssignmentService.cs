using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class CrewAssignmentService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IComplianceService complianceService,
        ILogger<CrewAssignmentService> logger) : ICrewAssignmentService
    {
        public async Task<List<CrewAssignment>> GetActiveByVesselAsync(int vesselId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.CrewAssignments.AsNoTracking()
                .Include(a => a.CrewMember)
                .Where(a => a.VesselId == vesselId && a.Status == CrewAssignmentStatus.SignedOn)
                .OrderByDescending(a => a.SignOnDateUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CrewAssignment>> GetByCrewMemberAsync(int crewMemberId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.CrewAssignments.AsNoTracking()
                .Include(a => a.Vessel)
                .Where(a => a.CrewMemberId == crewMemberId)
                .OrderByDescending(a => a.SignOnDateUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<CrewAssignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.CrewAssignments.AsNoTracking()
                .Include(a => a.Vessel)
                .Include(a => a.CrewMember)
                .Include(a => a.ApprovingComplianceResult)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<CrewAssignmentResult> SignOnAsync(int crewMemberId, int vesselId, CrewRank rankOnAssignment, string? portOfSignOn, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                var compliance = await complianceService.EvaluateCrewMemberAsync(crewMemberId, vesselId, rankOnAssignment, performedByUserId, cancellationToken);

                if (compliance.State == ComplianceState.NonCompliant || compliance.State == ComplianceState.Expired)
                {
                    logger.LogWarning("Sign-on rejected for crew {Crew} on vessel {Vessel}: {State}", crewMemberId, vesselId, compliance.State);
                    return new CrewAssignmentResult
                    {
                        Succeeded = false,
                        ComplianceResult = compliance,
                        Message = $"Sign-on rejected due to non-compliance. {compliance.Summary}"
                    };
                }

                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                bool alreadySignedOn = await db.CrewAssignments.AnyAsync(a =>
                    a.CrewMemberId == crewMemberId && a.Status == CrewAssignmentStatus.SignedOn, cancellationToken);
                if (alreadySignedOn)
                {
                    return new CrewAssignmentResult
                    {
                        Succeeded = false,
                        ComplianceResult = compliance,
                        Message = "Crew member is already signed on to a vessel."
                    };
                }

                var nowUtc = DateTime.UtcNow;
                var assignment = new CrewAssignment
                {
                    CrewMemberId = crewMemberId,
                    VesselId = vesselId,
                    RankOnAssignment = rankOnAssignment,
                    SignOnDateUtc = nowUtc,
                    PortOfSignOn = portOfSignOn,
                    Status = CrewAssignmentStatus.SignedOn,
                    ApprovingComplianceResultId = compliance.Id,
                    CreatedOnUtc = nowUtc,
                    CreatedByUserId = performedByUserId
                };
                db.CrewAssignments.Add(assignment);

                db.ComplianceAuditEntries.Add(new ComplianceAuditEntry
                {
                    Action = ComplianceAuditAction.CrewSignOnEvaluated,
                    CrewMemberId = crewMemberId,
                    VesselId = vesselId,
                    ComplianceResultId = compliance.Id,
                    Details = $"Sign-on approved. State={compliance.State}.",
                    OccurredOnUtc = nowUtc,
                    PerformedByUserId = performedByUserId
                });

                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Crew {Crew} signed on to vessel {Vessel}", crewMemberId, vesselId);
                return new CrewAssignmentResult
                {
                    Succeeded = true,
                    Assignment = assignment,
                    ComplianceResult = compliance,
                    Message = "Sign-on completed."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SignOnAsync failed for crew {Crew} vessel {Vessel}", crewMemberId, vesselId);
                throw;
            }
        }

        public async Task<CrewAssignment> SignOffAsync(int assignmentId, string? portOfSignOff, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var assignment = await db.CrewAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken)
                                 ?? throw new InvalidOperationException($"CrewAssignment {assignmentId} not found.");
                if (assignment.Status != CrewAssignmentStatus.SignedOn)
                    throw new InvalidOperationException("Assignment is not currently active.");

                var nowUtc = DateTime.UtcNow;
                assignment.Status = CrewAssignmentStatus.SignedOff;
                assignment.SignOffDateUtc = nowUtc;
                assignment.PortOfSignOff = portOfSignOff;
                assignment.ModifiedOnUtc = nowUtc;
                assignment.ModifiedByUserId = performedByUserId;

                db.ComplianceAuditEntries.Add(new ComplianceAuditEntry
                {
                    Action = ComplianceAuditAction.CrewSignOffRecorded,
                    CrewMemberId = assignment.CrewMemberId,
                    VesselId = assignment.VesselId,
                    Details = $"Signed off at {portOfSignOff ?? "(not specified)"}.",
                    OccurredOnUtc = nowUtc,
                    PerformedByUserId = performedByUserId
                });

                await db.SaveChangesAsync(cancellationToken);
                return assignment;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SignOffAsync failed for assignment {Id}", assignmentId);
                throw;
            }
        }

        public async Task CancelAsync(int assignmentId, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var assignment = await db.CrewAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
            if (assignment is null) return;
            assignment.Status = CrewAssignmentStatus.Cancelled;
            assignment.ModifiedOnUtc = DateTime.UtcNow;
            assignment.ModifiedByUserId = performedByUserId;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
