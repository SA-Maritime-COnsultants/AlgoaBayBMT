using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<List<BunkeringCompany>> GetCompaniesAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<BunkeringCompany>> CreateCompanyAsync(BunkeringCompany company, CancellationToken cancellationToken = default);
        Task<OperationResult<BunkeringCompany>> UpdateCompanyAsync(BunkeringCompany company, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteCompanyAsync(int companyId, CancellationToken cancellationToken = default);
        Task<BunkeringCompany?> GetCompanyAsync(int companyId, CancellationToken cancellationToken = default);
    }
}
