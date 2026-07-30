namespace AlgoaBayBMT.Services.Interfaces
{
    /// <summary>
    /// Placeholder interface for future training compliance email notifications.
    /// Implement a concrete service when the training reminder workflow is ready.
    /// </summary>
    public interface ITrainingComplianceNotifier
    {
        Task NotifyExpiredTrainingAsync(string userId, string email, CancellationToken cancellationToken = default);
        Task NotifyExpiringTrainingAsync(string userId, string email, DateTime expiresOn, CancellationToken cancellationToken = default);
    }
}
