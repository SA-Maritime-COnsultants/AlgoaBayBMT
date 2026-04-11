using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services.Crew
{
    public class CrewAssignmentService(
        ApplicationDbContext dbContext,
        ICrewChangeHistoryService historyService) : ICrewAssignmentService
    {
        public Task<List<CrewDeployment>> GetActiveCrewAsync(int vesselId, CancellationToken cancellationToken = default) =>
            dbContext.CrewDeployments
                .AsNoTracking()
                .Include(x => x.Vessel)
                .Where(x => x.VesselId == vesselId && x.Status == DeploymentStatus.Active)
                .OrderBy(x => x.OnboardRoleName)
                .ToListAsync(cancellationToken);

        public Task<CrewDeployment?> GetCurrentAssignmentAsync(string userId, CancellationToken cancellationToken = default) =>
            dbContext.CrewDeployments
                .AsNoTracking()
                .Include(x => x.Vessel)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == DeploymentStatus.Active, cancellationToken);

        public async Task<int?> GetCurrentVesselIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var deployment = await dbContext.CrewDeployments
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Status == DeploymentStatus.Active)
                .Select(x => (int?)x.VesselId)
                .FirstOrDefaultAsync(cancellationToken);
            return deployment;
        }

        public Task<List<ApplicationUser>> SearchUnassignedCrewAsync(string? searchTerm, CancellationToken cancellationToken = default)
        {
            var activeUserIds = dbContext.CrewDeployments
                .Where(x => x.Status == DeploymentStatus.Active)
                .Select(x => x.UserId);

            var query = dbContext.Users
                .AsNoTracking()
                .Include(x => x.CrewMemberDetails)
                .Where(x => x.IsCrew && x.IsActive && !activeUserIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.Contains(searchTerm)) ||
                    (x.Email != null && x.Email.Contains(searchTerm)) ||
                    (x.SidNumber != null && x.SidNumber.Contains(searchTerm)) ||
                    (x.CrewMemberDetails != null && x.CrewMemberDetails.PassportNumber != null && x.CrewMemberDetails.PassportNumber.Contains(searchTerm)));
            }

            return query.OrderBy(x => x.FullName).ToListAsync(cancellationToken);
        }

        public async Task<OperationResult<CrewDeployment>> SignOnAsync(SignOnRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return OperationResult<CrewDeployment>.Failure("User is required.");

            if (string.IsNullOrWhiteSpace(request.EmbarkationPort))
                return OperationResult<CrewDeployment>.Failure("Embarkation port is required.");

            if (request.Qualification is null)
                return OperationResult<CrewDeployment>.Failure("Qualification is required.");

            if (request.OnBoardRole is null)
                return OperationResult<CrewDeployment>.Failure("OnBoard Role is required.");

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            if (user is null)
                return OperationResult<CrewDeployment>.Failure("Crew member not found.");

            var vessel = await dbContext.Vessels.FirstOrDefaultAsync(x => x.Id == request.VesselId, cancellationToken);
            if (vessel is null)
                return OperationResult<CrewDeployment>.Failure("Vessel not found.");

            var hasActive = await dbContext.CrewDeployments.AnyAsync(
                x => x.UserId == request.UserId && x.Status == DeploymentStatus.Active, cancellationToken);
            if (hasActive)
                return OperationResult<CrewDeployment>.Failure("This crew member already has an active deployment. Sign off from the current vessel first.");

            user.Qualification = request.Qualification;
            user.CrewRank = request.Qualification.Value.ToCrewRank();

            var onboardRoleName = request.OnBoardRole.Value.GetDisplayName();
            var onboardRoleType = request.OnBoardRole.Value.ToVesselRoleType();

            var deployment = new CrewDeployment
            {
                UserId = request.UserId,
                VesselId = request.VesselId,
                OnboardRoleName = onboardRoleName,
                OnboardRoleType = onboardRoleType,
                Status = DeploymentStatus.Active,
                StartedOnUtc = request.EmbarkationDate,
                EmbarkationPort = request.EmbarkationPort,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                MedicalFitnessExpiry = request.MedicalFitnessExpiry,
                VaccinationStatus = request.VaccinationStatus,
                Duties = request.Duties,
                Notes = request.Notes,
                DeployedByUserId = request.DeployedByUserId
            };

            dbContext.CrewDeployments.Add(deployment);
            await dbContext.SaveChangesAsync(cancellationToken);

            await historyService.RecordAsync(new CrewChangeHistory
            {
                VesselId = request.VesselId,
                UserId = request.UserId,
                DeploymentId = deployment.Id,
                ActionType = "SignOn",
                ChangedByUserId = request.DeployedByUserId,
                ChangedByName = string.Empty,
                ChangedBySurname = string.Empty,
                ChangedByRank = onboardRoleName,
                ChangedOnUtc = DateTime.UtcNow,
                Notes = $"Signed on at {request.EmbarkationPort} as {onboardRoleName}"
            }, cancellationToken);

            return OperationResult<CrewDeployment>.Success(deployment, $"{user.FullName ?? user.Email} signed on to {vessel.Name} as {onboardRoleName}.");
        }

        public async Task<OperationResult> SignOffAsync(SignOffRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.DisembarkationPort))
                return OperationResult.Failure("Disembarkation port is required.");

            var deployment = await dbContext.CrewDeployments
                .Include(x => x.Vessel)
                .FirstOrDefaultAsync(x => x.Id == request.DeploymentId, cancellationToken);

            if (deployment is null)
                return OperationResult.Failure("Deployment not found.");

            if (deployment.Status != DeploymentStatus.Active)
                return OperationResult.Failure("This deployment is not currently active.");

            deployment.Status = DeploymentStatus.Completed;
            deployment.EndedOnUtc = request.DisembarkationDate;
            deployment.DisembarkationPort = request.DisembarkationPort;
            deployment.DisembarkationReason = request.DisembarkationReason;
            if (!string.IsNullOrWhiteSpace(request.Notes))
                deployment.Notes = request.Notes;

            await dbContext.SaveChangesAsync(cancellationToken);

            await historyService.RecordAsync(new CrewChangeHistory
            {
                VesselId = deployment.VesselId,
                UserId = deployment.UserId,
                DeploymentId = deployment.Id,
                ActionType = "SignOff",
                ChangedByUserId = request.SignedOffByUserId,
                ChangedByName = string.Empty,
                ChangedBySurname = string.Empty,
                ChangedByRank = deployment.OnboardRoleName,
                ChangedOnUtc = DateTime.UtcNow,
                Notes = $"Signed off at {request.DisembarkationPort}. Reason: {request.DisembarkationReason ?? "Not specified"}"
            }, cancellationToken);

            return OperationResult.Success("Crew member signed off.");
        }
    }
}
