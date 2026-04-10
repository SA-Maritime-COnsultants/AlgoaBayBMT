using AlgoaBayBMT.Data;
using Microsoft.AspNetCore.Identity;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IApplicationEmailService : IEmailSender<ApplicationUser>
    {
        Task SendWelcomeEmailAsync(ApplicationUser user, string portalBaseUrl, CancellationToken cancellationToken = default);
    }
}
