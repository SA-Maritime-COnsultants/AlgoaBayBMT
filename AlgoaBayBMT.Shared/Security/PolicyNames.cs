namespace AlgoaBayBMT.Shared.Security
{
    public static class PolicyNames
    {
        public const string AdminOnly = nameof(AdminOnly);
        public const string AdminOrCompanyManager = nameof(AdminOrCompanyManager);
        public const string CrewComplianceManagement = nameof(CrewComplianceManagement);
        public const string TrainingApproval = nameof(TrainingApproval);
        public const string BillingManagement = nameof(BillingManagement);
        public const string VesselCommand = nameof(VesselCommand);
        public const string AuditAuthorities = nameof(AuditAuthorities);
        public const string AreaScopedAccess = nameof(AreaScopedAccess);
        public const string CrewListAccess = nameof(CrewListAccess);
        public const string VesselCrewListsAccess = nameof(VesselCrewListsAccess);
    }
}
