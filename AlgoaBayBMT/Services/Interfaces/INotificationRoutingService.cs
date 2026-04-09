using AlgoaBayBMT.Services.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface INotificationRoutingService
    {
        Task<List<NotificationRecipient>> ResolveRecipientsAsync(int operationalAreaId, string notificationCategory, CancellationToken cancellationToken = default);
    }
}
