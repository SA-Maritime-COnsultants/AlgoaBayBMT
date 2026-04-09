namespace AlgoaBayBMT.Shared.Security
{
    public static class RoleNames
    {
        public const string Admin = "ADMIN";
        public const string SeniorManager = "SENIOR_MANAGER";
        public const string Officer = "OFFICER";
        public const string Crew = "CREW";
        public const string Poac = "POAC";
        public const string CompanyManager = "COMPANY_MANAGER";
        public const string CompanyUser = "COMPANY_USER";
        public const string Dffe = "DFFE";
        public const string Tnpa = "TNPA";
        public const string Samsa = "SAMSA";

        public static readonly string[] All =
        [
            Admin,
            SeniorManager,
            Officer,
            Crew,
            Poac,
            CompanyManager,
            CompanyUser,
            Dffe,
            Tnpa,
            Samsa
        ];

        public static readonly string[] AuditAuthorities =
        [
            Dffe,
            Tnpa,
            Samsa
        ];
    }
}
