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

        /// <summary>Returns the Baseline (WITHOUT measures) trajectory for a run.</summary>
        Task<IReadOnlyList<OilSpillTrajectoryPoint>> GetTrajectoryAsync(int modelRunId);

        /// <summary>Returns the trajectory for a specific scenario (Baseline or Mitigated).</summary>
        Task<IReadOnlyList<OilSpillTrajectoryPoint>> GetTrajectoryAsync(
            int modelRunId, OilSpillScenarioKind scenario);

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

        /// <summary>Returns every form of the given type for an incident (newest first).</summary>
        Task<IReadOnlyList<IncidentForm>> GetFormsByTypeAsync(int incidentId, IncidentFormType formType);

        /// <summary>
        /// Creates a new incident form (used by the "New Form" / "New Entry" UI actions). Returns the
        /// persisted form so the caller can immediately open it in the editor.
        /// </summary>
        Task<IncidentForm> CreateFormAsync(
            int incidentId,
            IncidentFormType formType,
            string jsonData,
            string createdBy,
            int? operationalPeriodId = null,
            string? personName = null,
            bool linkToSitrep = false);

        /// <summary>Persists JSON payload changes for an existing form and optionally updates status.</summary>
        Task<bool> UpdateFormAsync(int formId, string jsonData, IncidentFormStatus? status = null);

        /// <summary>Deletes an incident form (e.g. a mistaken ICS-214 entry). Returns false if missing.</summary>
        Task<bool> DeleteFormAsync(int formId);

        /// <summary>
        /// Adds a single ICS-214 activity-log entry as its own form record so each contributor can
        /// log independently. Entries are grouped by <paramref name="operationalPeriodId"/>.
        /// </summary>
        Task<IncidentForm> AddActivityLogEntryAsync(
            int incidentId,
            string personName,
            string activity,
            string? notes,
            int operationalPeriodId,
            string createdBy);

        /// <summary>
        /// Auto-generates an ICS-204 Assignment List when a response measure is deployed. Each call
        /// creates a new assignment record scoped to the operational period and division.
        /// </summary>
        Task<IncidentForm> EnsureAssignmentListAsync(
            int incidentId,
            OilSpillActionType actionType,
            string division,
            int operationalPeriodId,
            string createdBy);

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
