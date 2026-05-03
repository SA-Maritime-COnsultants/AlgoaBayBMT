using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class OperatorService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IOperatorService
    {
        public async Task<List<BunkerOperator>> GetAllAsync()
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.BunkerOperators
                .AsNoTracking()
                .Include(x => x.AreaAssignments)
                    .ThenInclude(x => x.AreaOfOperation)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<BunkerOperator?> GetByIdAsync(int id)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.BunkerOperators
                .AsNoTracking()
                .Include(x => x.AreaAssignments)
                    .ThenInclude(x => x.AreaOfOperation)
                .Include(x => x.Barges)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BunkerOperator> CreateAsync(BunkerOperator op)
        {
            ArgumentNullException.ThrowIfNull(op);
            Normalize(op);
            Validate(op);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            dbContext.BunkerOperators.Add(op);
            await dbContext.SaveChangesAsync();
            return op;
        }

        public async Task UpdateAsync(BunkerOperator op)
        {
            ArgumentNullException.ThrowIfNull(op);
            Normalize(op);
            Validate(op);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.BunkerOperators.FirstOrDefaultAsync(x => x.Id == op.Id)
                ?? throw new InvalidOperationException($"Bunker Operator with id {op.Id} was not found.");

            existing.Name = op.Name;
            existing.CompanyRegistrationNumber = op.CompanyRegistrationNumber;
            existing.PhysicalAddress = op.PhysicalAddress;
            existing.ContactPerson = op.ContactPerson;
            existing.Email = op.Email;
            existing.Phone = op.Phone;
            existing.EmergencyContactNumber = op.EmergencyContactNumber;
            existing.IsActive = op.IsActive;
            await dbContext.SaveChangesAsync();
        }

        public async Task AssignToAreaAsync(int operatorId, int areaId, DateTime from)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var opExists = await dbContext.BunkerOperators.AnyAsync(x => x.Id == operatorId && x.IsActive);
            if (!opExists)
            {
                throw new InvalidOperationException("Active bunker operator was not found.");
            }

            var areaExists = await dbContext.AreasOfOperation.AnyAsync(x => x.Id == areaId && x.IsActive);
            if (!areaExists)
            {
                throw new InvalidOperationException("Active area of operation was not found.");
            }

            var overlaps = await dbContext.OperatorAreaAssignments.AnyAsync(x =>
                x.BunkerOperatorId == operatorId &&
                x.AreaOfOperationId == areaId &&
                x.AssignedFrom <= from &&
                (x.AssignedTo == null || x.AssignedTo >= from));

            if (overlaps)
            {
                throw new InvalidOperationException("The operator is already assigned to this area for the selected date.");
            }

            dbContext.OperatorAreaAssignments.Add(new OperatorAreaAssignment
            {
                BunkerOperatorId = operatorId,
                AreaOfOperationId = areaId,
                AssignedFrom = from
            });

            await dbContext.SaveChangesAsync();
        }

        public async Task EndAreaAssignmentAsync(int assignmentId, DateTime to)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var assignment = await dbContext.OperatorAreaAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId)
                ?? throw new InvalidOperationException($"Operator area assignment with id {assignmentId} was not found.");

            if (assignment.AssignedTo.HasValue)
            {
                throw new InvalidOperationException("This operator area assignment has already ended.");
            }

            if (to < assignment.AssignedFrom)
            {
                throw new InvalidOperationException("Assignment end date cannot be earlier than the assignment start date.");
            }

            assignment.AssignedTo = to;
            await dbContext.SaveChangesAsync();
        }

        private static void Normalize(BunkerOperator op)
        {
            op.Name = op.Name.Trim();
        }

        private static void Validate(BunkerOperator op)
        {
            if (string.IsNullOrWhiteSpace(op.Name))
            {
                throw new InvalidOperationException("Bunker operator name is required.");
            }
        }
    }
}
