namespace AlgoaBayBMT.Shared.Security
{
    public static class PolicyNames
    {
        public const string AdminOnly = nameof(AdminOnly);
        public const string AdminOrCompanyManager = nameof(AdminOrCompanyManager);
        public const string VesselCommand = nameof(VesselCommand);
        public const string AuditAuthorities = nameof(AuditAuthorities);
        public const string AreaScopedAccess = nameof(AreaScopedAccess);
    }
}
