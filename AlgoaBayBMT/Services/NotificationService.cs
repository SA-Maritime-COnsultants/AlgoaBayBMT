using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class NotificationService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IServiceProvider serviceProvider,
        ILogger<NotificationService> logger) : INotificationService
    {
        public async Task<NotificationMessage> QueueAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            message.CreatedOnUtc = DateTime.UtcNow;
            message.Status = NotificationStatus.Pending;
            db.NotificationMessages.Add(message);
            await db.SaveChangesAsync(cancellationToken);
            return message;
        }

        public Task<NotificationMessage> QueueAsync(
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
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(new NotificationMessage
            {
                RecipientUserId = recipientUserId,
                RecipientEmail = recipientEmail,
                RecipientDisplayName = recipientDisplayName,
                Subject = subject,
                Body = body,
                Category = category,
                Channel = channel,
                RelatedCrewMemberId = relatedCrewMemberId,
                RelatedVesselId = relatedVesselId,
                RelatedComplianceResultId = relatedComplianceResultId,
                RelatedInvoiceId = relatedInvoiceId,
                RelatedExpiryEventId = relatedExpiryEventId
            }, cancellationToken);
        }

        public async Task<int> DispatchPendingAsync(int batchSize = 50, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0) batchSize = 50;
            int dispatched = 0;
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var pending = await db.NotificationMessages
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedOnUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (pending.Count == 0) return 0;

            // Resolve email sender lazily so missing SMTP config simply falls back to logging.
            IEmailSender? emailSender = null;
            try { emailSender = serviceProvider.GetService<IEmailSender>(); }
            catch (Exception ex) { logger.LogWarning(ex, "Email sender unavailable; notifications will be in-app only"); }

            foreach (var msg in pending)
            {
                try
                {
                    var sendEmail = msg.Channel is NotificationChannel.Email or NotificationChannel.Both
                                    && !string.IsNullOrWhiteSpace(msg.RecipientEmail);

                    if (sendEmail)
                    {
                        if (emailSender is not null)
                        {
                            await emailSender.SendEmailAsync(msg.RecipientEmail!, msg.Subject, msg.Body);
                        }
                        else
                        {
                            logger.LogInformation("[Notification fallback] {Category} -> {Email}: {Subject}", msg.Category, msg.RecipientEmail, msg.Subject);
                        }
                    }

                    msg.Status = NotificationStatus.Sent;
                    msg.SentOnUtc = DateTime.UtcNow;
                    dispatched++;
                }
                catch (Exception ex)
                {
                    msg.RetryCount++;
                    msg.FailureReason = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                    if (msg.RetryCount >= 5)
                    {
                        msg.Status = NotificationStatus.Failed;
                    }
                    logger.LogError(ex, "Failed to dispatch notification {Id}", msg.Id);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            return dispatched;
        }

        public async Task<List<NotificationMessage>> GetForUserAsync(string userId, bool unreadOnly = false, int take = 50, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var q = db.NotificationMessages.AsNoTracking().Where(n => n.RecipientUserId == userId);
            if (unreadOnly) q = q.Where(n => n.Status != NotificationStatus.Read);
            return await q.OrderByDescending(n => n.CreatedOnUtc).Take(take).ToListAsync(cancellationToken);
        }

        public async Task MarkReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var msg = await db.NotificationMessages.FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId, cancellationToken);
            if (msg is null) return;
            msg.Status = NotificationStatus.Read;
            msg.ReadOnUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
