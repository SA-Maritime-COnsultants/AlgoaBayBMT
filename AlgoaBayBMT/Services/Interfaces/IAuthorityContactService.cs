using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IAuthorityContactService
    {
        Task<List<AuthorityContact>> GetAuthorityContactsAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<AuthorityContact>> CreateAuthorityContactAsync(AuthorityContact contact, CancellationToken cancellationToken = default);
        Task<OperationResult<AuthorityContact>> UpdateAuthorityContactAsync(AuthorityContact contact, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAuthorityContactAsync(int contactId, CancellationToken cancellationToken = default);
    }
}
