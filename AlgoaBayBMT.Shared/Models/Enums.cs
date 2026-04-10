using System;
using System.ComponentModel.DataAnnotations;

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

    public enum CrewRank
    {
        [Display(Name = "Captain")]
        Captain = 0,

        [Display(Name = "Chief Officer")]
        ChiefOfficer = 1,

        [Display(Name = "Chief Engineer")]
        ChiefEngineer = 2,

        [Display(Name = "2nd Engineer")]
        SecondEngineer = 3,

        [Display(Name = "3rd Engineer")]
        ThirdEngineer = 4,

        [Display(Name = "Bosun")]
        Bosun = 5,

        [Display(Name = "Able Seaman")]
        AbleSeaman = 6,

        [Display(Name = "Ordinary Seaman")]
        OrdinarySeaman = 7,

        [Display(Name = "Deck Cadet")]
        DeckCadet = 8,

        [Display(Name = "Engine Cadet")]
        EngineCadet = 9,

        [Display(Name = "Cook")]
        Cook = 10,

        [Display(Name = "Steward")]
        Steward = 11,

        [Display(Name = "Master")]
        Master = 12
    }
}
