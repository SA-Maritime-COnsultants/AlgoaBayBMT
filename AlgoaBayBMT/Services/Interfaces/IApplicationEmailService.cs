using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IApplicationEmailService : IEmailSender<ApplicationUser>
    {
        Task SendWelcomeEmailAsync(ApplicationUser user, string portalBaseUrl, CancellationToken cancellationToken = default);
        Task<OperationResult> SendWelcomeEmailWithResultAsync(ApplicationUser user, string portalBaseUrl, CancellationToken cancellationToken = default);
    }
}
