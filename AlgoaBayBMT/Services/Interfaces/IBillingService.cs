using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IBillingService
    {
        Task<OperatorBillingAccount?> GetAccountAsync(int bunkerOperatorId, CancellationToken cancellationToken = default);
        Task<OperatorBillingAccount> UpsertAccountAsync(OperatorBillingAccount account, string? performedByUserId, CancellationToken cancellationToken = default);

        Task<List<Invoice>> GetInvoicesForOperatorAsync(int bunkerOperatorId, CancellationToken cancellationToken = default);
        Task<Invoice?> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a draft invoice for compliance remediation (auto-assigned training).
        /// Adds one line item per assigned course at the operator-account default unit price.
        /// </summary>
        Task<Invoice> GenerateRemediationInvoiceAsync(int bunkerOperatorId, int? crewMemberId, int? vesselId, IEnumerable<Guid> courseIds, decimal unitPrice, string? performedByUserId, CancellationToken cancellationToken = default);

        Task<Invoice> IssueAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default);
        Task<Invoice> MarkPaidAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default);
        Task<Invoice> CancelAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default);
        Task<List<Invoice>> GetInvoicesByIdsAsync(IEnumerable<int> invoiceIds, CancellationToken cancellationToken = default);
    }
}
