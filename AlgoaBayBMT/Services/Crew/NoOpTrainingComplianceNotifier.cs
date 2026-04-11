using AlgoaBayBMT.Services.Interfaces;

namespace AlgoaBayBMT.Services.Crew
{
    /// <summary>
    /// No-op placeholder. Replace with a real implementation when the training reminder
    /// email workflow is ready (e.g. SendGrid / MailKit + background job).
    /// </summary>
    public sealed class NoOpTrainingComplianceNotifier : ITrainingComplianceNotifier
    {
        public Task NotifyExpiredTrainingAsync(string userId, string email, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task NotifyExpiringTrainingAsync(string userId, string email, DateTime expiresOn, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
