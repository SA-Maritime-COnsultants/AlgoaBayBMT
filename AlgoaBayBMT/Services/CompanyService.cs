using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class CompanyService(ApplicationDbContext dbContext) : ICompanyService
    {
        public Task<List<BunkeringCompany>> GetCompaniesAsync(CancellationToken cancellationToken = default) =>
            dbContext.BunkeringCompanies.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

        public Task<BunkeringCompany?> GetCompanyAsync(int companyId, CancellationToken cancellationToken = default) =>
            dbContext.BunkeringCompanies.Include(x => x.AreaAssignments).ThenInclude(x => x.OperationalArea)
                .FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken);

        public async Task<OperationResult<BunkeringCompany>> CreateCompanyAsync(BunkeringCompany company, CancellationToken cancellationToken = default)
        {
            if (await dbContext.BunkeringCompanies.AnyAsync(x => x.RegistrationNumber == company.RegistrationNumber, cancellationToken))
            {
                return OperationResult<BunkeringCompany>.Failure("Registration number already exists.");
            }

            dbContext.BunkeringCompanies.Add(company);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<BunkeringCompany>.Success(company, "Company created.");
        }

        public async Task<OperationResult<BunkeringCompany>> UpdateCompanyAsync(BunkeringCompany company, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.BunkeringCompanies.FindAsync([company.Id], cancellationToken);
            if (existing is null)
            {
                return OperationResult<BunkeringCompany>.Failure("Company not found.");
            }

            existing.Name = company.Name;
            existing.RegistrationNumber = company.RegistrationNumber;
            existing.ContactEmail = company.ContactEmail;
            existing.ContactPhone = company.ContactPhone;
            existing.IsActive = company.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<BunkeringCompany>.Success(existing, "Company updated.");
        }

        public async Task<OperationResult> DeleteCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var company = await dbContext.BunkeringCompanies.FindAsync([companyId], cancellationToken);
            if (company is null)
            {
                return OperationResult.Failure("Company not found.");
            }

            dbContext.BunkeringCompanies.Remove(company);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Company deleted.");
        }
    }
}
