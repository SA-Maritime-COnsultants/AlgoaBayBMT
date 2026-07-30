namespace AlgoaBayBMT.Shared.Security
{
    public static class RoleNames
    {
        public const string Admin = "ADMIN";
        public const string SeniorManager = "SENIOR_MANAGER";
        public const string Officer = "OFFICER";
        public const string Crew = "CREW";
        public const string Responder = "RESPONDER";
        public const string Poac = "POAC";
        public const string CompanyManager = "COMPANY_MANAGER";
        public const string CompanyUser = "COMPANY_USER";
        public const string Customer = "CUSTOMER";
        public const string Dffe = "DFFE";
        public const string Tnpa = "TNPA";
        public const string Samsa = "SAMSA";
        public const string Captain = "CAPTAIN";
        public const string Co = "CO";

        public static readonly string[] All =
        [
            Admin,
            SeniorManager,
            Officer,
            Crew,
            Responder,
            Poac,
            CompanyManager,
            CompanyUser,
            Customer,
            Dffe,
            Tnpa,
            Samsa,
            Captain,
            Co
        ];

        public static readonly string[] CrewCommand =
        [
            Admin,
            CompanyManager,
            Captain,
            Co
        ];

        public static readonly string[] AuditAuthorities =
        [
            Dffe,
            Tnpa,
            Samsa
        ];

        public static readonly string[] TrainingFullAccess =
        [
            Admin,
            Dffe,
            Samsa
        ];
    }
}
