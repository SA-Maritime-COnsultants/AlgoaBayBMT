using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationMessage> QueueAsync(NotificationMessage message, CancellationToken cancellationToken = default);

        Task<NotificationMessage> QueueAsync(
            string recipientUserId,
            string? recipientEmail,
            string? recipientDisplayName,
            string subject,
            string body,
            NotificationCategory category,
            NotificationChannel channel = NotificationChannel.InApp,
            int? relatedCrewMemberId = null,
            int? relatedVesselId = null,
            int? relatedComplianceResultId = null,
            int? relatedInvoiceId = null,
            int? relatedExpiryEventId = null,
            CancellationToken cancellationToken = default);

        Task<int> DispatchPendingAsync(int batchSize = 50, CancellationToken cancellationToken = default);

        Task<List<NotificationMessage>> GetForUserAsync(string userId, bool unreadOnly = false, int take = 50, CancellationToken cancellationToken = default);
        Task MarkReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default);
    }
}
