using System;

namespace AlgoaBayBMT.Shared.Models
{
    public enum ApprovalStatus
    {
        PendingEmailConfirmation = 0,
        PendingAccountApproval = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum VesselRoleType
    {
        Crew = 0,
        Officer = 1,
        ChiefOfficer = 2,
        Master = 3,
        Poac = 4,
        CompanyManager = 5,
        CompanyUser = 6
    }

    public enum AreaType
    {
        Area = 0,
        Port = 1,
        Bay = 2,
        Anchorage = 3
    }

    public enum DeploymentStatus
    {
        Planned = 0,
        Active = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum ComplianceState
    {
        Unknown = 0,
        Valid = 1,
        Expiring = 2,
        Expired = 3,
        NonCompliant = 4
    }
}
