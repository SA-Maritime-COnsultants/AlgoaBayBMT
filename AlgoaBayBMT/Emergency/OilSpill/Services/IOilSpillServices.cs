using AlgoaBayBMT.Emergency.OilSpill.DTOs;
using AlgoaBayBMT.Emergency.OilSpill.Models;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    public interface IOilSpillService
    {
        Task<OilSpillIncident> CreateSpillAsync(CreateOilSpillRequest request);
        Task<OilSpillIncident?> GetSpillByIdAsync(int id);
        Task<IReadOnlyList<OilSpillIncident>> GetActiveSpillsAsync();
        Task<IReadOnlyList<OilSpillIncident>> GetSpillsByOperationAsync(int bunkeringOperationId);
        Task<bool> UpdateSpillAsync(UpdateOilSpillRequest request);
        Task<bool> CloseSpillAsync(int id, DateTime closedAt);
    }

    public interface IOilSpillModelService
    {
        Task<OilSpillModelRun> RunModelAsync(OilSpillModelRunRequest request);
        Task<IReadOnlyList<OilSpillModelRun>> GetModelRunsForSpillAsync(int spillId);
        Task<OilSpillModelRun?> GetModelRunByIdAsync(int modelRunId);
        Task<IReadOnlyList<OilSpillTrajectoryPoint>> GetTrajectoryAsync(int modelRunId);
        Task<bool> DeleteModelRunAsync(int modelRunId);
        Task RecordShorelineImpactAsync(int modelRunId, int impactIndex, DateTime impactTime);
    }

    public interface IOilSpillResponseService
    {
        Task<OilSpillResponseAction> AddResponseActionAsync(CreateOilSpillResponseActionRequest request);
        Task<IReadOnlyList<OilSpillResponseAction>> GetActionsForSpillAsync(int spillId);
        Task<bool> CloseActionAsync(int actionId, DateTime endTime);
    }

    public interface IIncidentFormService
    {
        /// <summary>Returns all incident-management forms for a spill, newest first.</summary>
        Task<IReadOnlyList<IncidentForm>> GetFormsForIncidentAsync(int incidentId);

        Task<IncidentForm?> GetFormByIdAsync(int formId);

        /// <summary>Returns the single form of the given type for an incident, if it exists.</summary>
        Task<IncidentForm?> GetFormByTypeAsync(int incidentId, IncidentFormType formType);

        /// <summary>Persists JSON payload changes for an existing form and optionally updates status.</summary>
        Task<bool> UpdateFormAsync(int formId, string jsonData, IncidentFormStatus? status = null);

        /// <summary>
        /// Creates the mandatory ICS-209 (and, when applicable, ICS-201) forms for a newly created
        /// incident. ICS-209 is always linked to the SITREP.
        /// </summary>
        Task EnsureInitialFormsAsync(int incidentId, string createdBy);

        /// <summary>
        /// Re-generates the ICS-209 status summary from the incident's latest model run so the
        /// SITREP always reflects current trajectory results.
        /// </summary>
        Task<IncidentForm?> SyncStatusSummaryAsync(int incidentId);

        /// <summary>
        /// Appends an ICS-214 activity-log entry for a response action, creating the ICS-214 form
        /// (and linking it to the SITREP) on first use.
        /// </summary>
        Task LogActivityAsync(int incidentId, string activity, string performedBy);

        /// <summary>Returns the forms flagged for inclusion in the SITREP export.</summary>
        Task<IReadOnlyList<IncidentForm>> GetSitrepLinkedFormsAsync(int incidentId);
    }
}
