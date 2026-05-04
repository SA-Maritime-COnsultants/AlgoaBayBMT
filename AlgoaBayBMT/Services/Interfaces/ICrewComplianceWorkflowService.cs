using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces;

public interface ICrewComplianceWorkflowService
{
    Task<CrewComplianceDashboardModel?> GetCrewDashboardAsync(int crewMemberId, int? vesselId = null, CancellationToken cancellationToken = default);
    Task<List<CrewComplianceDashboardModel>> GetCrewRegisterDashboardAsync(CancellationToken cancellationToken = default);
    Task<List<TrainingApprovalQueueItemModel>> GetPendingApprovalsAsync(int? operatorId = null, CancellationToken cancellationToken = default);
    Task<OperationResult<TrainingRegistrationSubmissionResultModel>> SubmitTrainingRegistrationAsync(TrainingRegistrationSelectionModel request, string? performedByUserId, CancellationToken cancellationToken = default);
    Task<OperationResult<TrainingApprovalResultModel>> ApproveRegistrationsAsync(IEnumerable<Guid> assignmentIds, string? performedByUserId, string portalBaseUrl, CancellationToken cancellationToken = default);
    Task<OperationResult> SynchronizeCompletedAssignmentsAsync(string userId, CancellationToken cancellationToken = default);
    Task<OperationResult> SynchronizeInvoicePaymentAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default);
}
