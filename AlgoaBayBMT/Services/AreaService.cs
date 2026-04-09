using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class AreaService(ApplicationDbContext dbContext) : IAreaService
    {
        public Task<List<OperationalArea>> GetAreasAsync(CancellationToken cancellationToken = default) =>
            dbContext.OperationalAreas.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

        public Task<List<Port>> GetPortsAsync(CancellationToken cancellationToken = default) =>
            dbContext.Ports.AsNoTracking().Include(x => x.OperationalArea).OrderBy(x => x.Name).ToListAsync(cancellationToken);

        public Task<List<Bay>> GetBaysAsync(CancellationToken cancellationToken = default) =>
            dbContext.Bays.AsNoTracking().Include(x => x.OperationalArea).Include(x => x.Port).OrderBy(x => x.Name).ToListAsync(cancellationToken);

        public Task<List<Anchorage>> GetAnchoragesAsync(CancellationToken cancellationToken = default) =>
            dbContext.Anchorages.AsNoTracking().Include(x => x.OperationalArea).Include(x => x.Port).Include(x => x.Bay).OrderBy(x => x.Name).ToListAsync(cancellationToken);

        public async Task<OperationResult<OperationalArea>> CreateAreaAsync(OperationalArea area, CancellationToken cancellationToken = default)
        {
            if (await dbContext.OperationalAreas.AnyAsync(x => x.Code == area.Code, cancellationToken))
            {
                return OperationResult<OperationalArea>.Failure("Area code already exists.");
            }

            dbContext.OperationalAreas.Add(area);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<OperationalArea>.Success(area, "Area created.");
        }

        public async Task<OperationResult<OperationalArea>> UpdateAreaAsync(OperationalArea area, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.OperationalAreas.FindAsync([area.Id], cancellationToken);
            if (existing is null)
            {
                return OperationResult<OperationalArea>.Failure("Area not found.");
            }

            existing.Name = area.Name;
            existing.Code = area.Code;
            existing.Description = area.Description;
            existing.IsActive = area.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<OperationalArea>.Success(existing, "Area updated.");
        }

        public async Task<OperationResult<Port>> CreatePortAsync(Port port, CancellationToken cancellationToken = default)
        {
            if (await dbContext.Ports.AnyAsync(x => x.OperationalAreaId == port.OperationalAreaId && x.Code == port.Code, cancellationToken))
            {
                return OperationResult<Port>.Failure("Port code already exists in this area.");
            }

            dbContext.Ports.Add(port);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Port>.Success(port, "Port created.");
        }

        public async Task<OperationResult<Port>> UpdatePortAsync(Port port, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Ports.FindAsync([port.Id], cancellationToken);
            if (existing is null)
            {
                return OperationResult<Port>.Failure("Port not found.");
            }

            existing.OperationalAreaId = port.OperationalAreaId;
            existing.Name = port.Name;
            existing.Code = port.Code;
            existing.IsActive = port.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Port>.Success(existing, "Port updated.");
        }

        public async Task<OperationResult<Bay>> CreateBayAsync(Bay bay, CancellationToken cancellationToken = default)
        {
            if (await dbContext.Bays.AnyAsync(x => x.OperationalAreaId == bay.OperationalAreaId && x.Code == bay.Code, cancellationToken))
            {
                return OperationResult<Bay>.Failure("Bay code already exists in this area.");
            }

            dbContext.Bays.Add(bay);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Bay>.Success(bay, "Bay created.");
        }

        public async Task<OperationResult<Bay>> UpdateBayAsync(Bay bay, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Bays.FindAsync([bay.Id], cancellationToken);
            if (existing is null)
            {
                return OperationResult<Bay>.Failure("Bay not found.");
            }

            existing.OperationalAreaId = bay.OperationalAreaId;
            existing.PortId = bay.PortId;
            existing.Name = bay.Name;
            existing.Code = bay.Code;
            existing.IsActive = bay.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Bay>.Success(existing, "Bay updated.");
        }

        public async Task<OperationResult<Anchorage>> CreateAnchorageAsync(Anchorage anchorage, CancellationToken cancellationToken = default)
        {
            if (await dbContext.Anchorages.AnyAsync(x => x.OperationalAreaId == anchorage.OperationalAreaId && x.Code == anchorage.Code, cancellationToken))
            {
                return OperationResult<Anchorage>.Failure("Anchorage code already exists in this area.");
            }

            dbContext.Anchorages.Add(anchorage);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Anchorage>.Success(anchorage, "Anchorage created.");
        }

        public async Task<OperationResult<Anchorage>> UpdateAnchorageAsync(Anchorage anchorage, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Anchorages.FindAsync([anchorage.Id], cancellationToken);
            if (existing is null)
            {
                return OperationResult<Anchorage>.Failure("Anchorage not found.");
            }

            existing.OperationalAreaId = anchorage.OperationalAreaId;
            existing.PortId = anchorage.PortId;
            existing.BayId = anchorage.BayId;
            existing.Name = anchorage.Name;
            existing.Code = anchorage.Code;
            existing.IsActive = anchorage.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Anchorage>.Success(existing, "Anchorage updated.");
        }

        public async Task<OperationResult> DeleteAreaAsync(int areaId, CancellationToken cancellationToken = default)
        {
            var area = await dbContext.OperationalAreas.FindAsync([areaId], cancellationToken);
            if (area is null)
            {
                return OperationResult.Failure("Area not found.");
            }

            dbContext.OperationalAreas.Remove(area);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Area deleted.");
        }

        public async Task<OperationResult> DeletePortAsync(int portId, CancellationToken cancellationToken = default)
        {
            var port = await dbContext.Ports.FindAsync([portId], cancellationToken);
            if (port is null)
            {
                return OperationResult.Failure("Port not found.");
            }

            dbContext.Ports.Remove(port);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Port deleted.");
        }

        public async Task<OperationResult> DeleteBayAsync(int bayId, CancellationToken cancellationToken = default)
        {
            var bay = await dbContext.Bays.FindAsync([bayId], cancellationToken);
            if (bay is null)
            {
                return OperationResult.Failure("Bay not found.");
            }

            dbContext.Bays.Remove(bay);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Bay deleted.");
        }

        public async Task<OperationResult> DeleteAnchorageAsync(int anchorageId, CancellationToken cancellationToken = default)
        {
            var anchorage = await dbContext.Anchorages.FindAsync([anchorageId], cancellationToken);
            if (anchorage is null)
            {
                return OperationResult.Failure("Anchorage not found.");
            }

            dbContext.Anchorages.Remove(anchorage);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Anchorage deleted.");
        }

        public async Task<OperationResult> AssignUserToAreaAsync(string userId, int areaId, bool isPrimary, string? assignedByUserId, CancellationToken cancellationToken = default)
        {
            var exists = await dbContext.UserAreaAssignments.AnyAsync(x => x.UserId == userId && x.OperationalAreaId == areaId, cancellationToken);
            if (exists)
            {
                return OperationResult.Failure("User is already assigned to this area.");
            }

            dbContext.UserAreaAssignments.Add(new UserAreaAssignment
            {
                UserId = userId,
                OperationalAreaId = areaId,
                IsPrimary = isPrimary,
                AssignedByUserId = assignedByUserId,
                IsActive = true
            });

            if (isPrimary)
            {
                var user = await dbContext.Users.FindAsync([userId], cancellationToken);
                if (user is not null)
                {
                    user.PrimaryAreaId = areaId;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("User area assignment created.");
        }

        public async Task<OperationResult> AssignCompanyToAreaAsync(int companyId, int areaId, string? assignedByUserId, CancellationToken cancellationToken = default)
        {
            var exists = await dbContext.CompanyAreaAssignments.AnyAsync(x => x.CompanyId == companyId && x.OperationalAreaId == areaId, cancellationToken);
            if (exists)
            {
                return OperationResult.Failure("Company is already assigned to this area.");
            }

            dbContext.CompanyAreaAssignments.Add(new CompanyAreaAssignment
            {
                CompanyId = companyId,
                OperationalAreaId = areaId,
                AssignedByUserId = assignedByUserId,
                IsActive = true
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Company area assignment created.");
        }

        public async Task<OperationResult> AssignAuthorityToAreaAsync(int authorityContactId, int areaId, string? assignedByUserId, CancellationToken cancellationToken = default)
        {
            var exists = await dbContext.AuthorityAreaAssignments.AnyAsync(x => x.AuthorityContactId == authorityContactId && x.OperationalAreaId == areaId, cancellationToken);
            if (exists)
            {
                return OperationResult.Failure("Authority contact is already assigned to this area.");
            }

            dbContext.AuthorityAreaAssignments.Add(new AuthorityAreaAssignment
            {
                AuthorityContactId = authorityContactId,
                OperationalAreaId = areaId,
                AssignedByUserId = assignedByUserId,
                IsActive = true
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Authority area assignment created.");
        }
    }
}
