using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IUserRegistrationService
    {
        Task<OperationResult<ApplicationUser>> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);
    }
}
