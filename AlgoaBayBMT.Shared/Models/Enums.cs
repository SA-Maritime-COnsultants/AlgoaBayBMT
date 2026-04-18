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

    public enum SeagoingCommercialQualification
    {
        [Display(Name = "Master Foreign Going")]
        MasterForeignGoing = 0,

        [Display(Name = "Chief Officer Foreign Going")]
        ChiefOfficerForeignGoing = 1,

        [Display(Name = "Officer in Charge of a Navigational Watch")]
        OfficerInChargeNavigationalWatch = 2,

        [Display(Name = "Able Seafarer Deck")]
        AbleSeafarerDeck = 3,

        [Display(Name = "Rating Forming Part of a Navigational Watch")]
        RatingFormingPartNavigationalWatch = 4,

        [Display(Name = "Chief Engineer Officer")]
        ChiefEngineerOfficer = 5,

        [Display(Name = "Second Engineer Officer")]
        SecondEngineerOfficer = 6,

        [Display(Name = "Officer in Charge of an Engineering Watch")]
        OfficerInChargeEngineeringWatch = 7,

        [Display(Name = "Electro-Technical Officer")]
        ElectroTechnicalOfficer = 8,

        [Display(Name = "Electro-Technical Rating")]
        ElectroTechnicalRating = 9,

        [Display(Name = "Able Seafarer Engine")]
        AbleSeafarerEngine = 10,

        [Display(Name = "Rating Forming Part of an Engineering Watch")]
        RatingFormingPartEngineeringWatch = 11,

        [Display(Name = "Bosun")]
        BosunQualification = 12,

        [Display(Name = "Pumpman")]
        PumpmanQualification = 13,

        [Display(Name = "Deck Cadet")]
        DeckCadetQualification = 14,

        [Display(Name = "Engine Cadet")]
        EngineCadetQualification = 15,

        [Display(Name = "General Purpose Rating")]
        GeneralPurposeRating = 16,

        [Display(Name = "Catering Rating")]
        CateringRating = 17,

        [Display(Name = "Ordinary Seaman")]
        OrdinarySeaman = 18,

        [Display(Name = "Wiper")]
        Wiper = 19,

        [Display(Name = "Motorman")]
        Motorman = 20,

        [Display(Name = "Fitter")]
        Fitter = 21,

        [Display(Name = "Refrigeration Engineer")]
        RefrigerationEngineer = 22,

        [Display(Name = "Electrician")]
        Electrician = 23,

        [Display(Name = "Crane Operator")]
        CraneOperator = 24,

        [Display(Name = "Pumpman Tanker")]
        PumpmanTanker = 25,

        [Display(Name = "Other")]
        Other = 26
    }

    public enum OnBoardRoles
    {
        [Display(Name = "Captain")]
        Captain = 0,

        [Display(Name = "Chief Officer")]
        ChiefOfficer = 1,

        [Display(Name = "Second Officer")]
        SecondOfficer = 2,

        [Display(Name = "Third Officer")]
        ThirdOfficer = 3,

        [Display(Name = "Junior Officer")]
        JuniorOfficer = 4,

        [Display(Name = "Deck Cadet")]
        DeckCadet = 5,

        [Display(Name = "Bosun")]
        Bosun = 6,

        [Display(Name = "Able Bodied Seaman")]
        AbleBodiedSeaman = 7,

        [Display(Name = "Ordinary Seaman")]
        OrdinarySeaman = 8,

        [Display(Name = "Deck Rating")]
        DeckRating = 9,

        [Display(Name = "Pumpman")]
        Pumpman = 10,

        [Display(Name = "Chief Engineer")]
        ChiefEngineer = 11,

        [Display(Name = "Second Engineer")]
        SecondEngineer = 12,

        [Display(Name = "Third Engineer")]
        ThirdEngineer = 13,

        [Display(Name = "Fourth Engineer")]
        FourthEngineer = 14,

        [Display(Name = "Junior Engineer")]
        JuniorEngineer = 15,

        [Display(Name = "Engine Cadet")]
        EngineCadet = 16,

        [Display(Name = "Electro-Technical Officer")]
        ElectroTechnicalOfficer = 17,

        [Display(Name = "Electrician")]
        Electrician = 18,

        [Display(Name = "Motorman")]
        Motorman = 19,

        [Display(Name = "Oiler")]
        Oiler = 20,

        [Display(Name = "Wiper")]
        Wiper = 21,

        [Display(Name = "Fitter")]
        Fitter = 22,

        [Display(Name = "Refrigeration Engineer")]
        RefrigerationEngineer = 23,

        [Display(Name = "Chief Cook")]
        ChiefCook = 24,

        [Display(Name = "Cook")]
        Cook = 25,

        [Display(Name = "Steward")]
        Steward = 26,

        [Display(Name = "Messman")]
        Messman = 27,

        [Display(Name = "POAC")]
        Poac = 28,

        [Display(Name = "Crane Operator")]
        CraneOperator = 29,

        [Display(Name = "Cargo Officer")]
        CargoOfficer = 30,

        [Display(Name = "Safety Officer")]
        SafetyOfficer = 31,

        [Display(Name = "Security Officer")]
        SecurityOfficer = 32,

        [Display(Name = "Environmental Officer")]
        EnvironmentalOfficer = 33,

        [Display(Name = "Medic")]
        Medic = 34,

        [Display(Name = "General Purpose Hand")]
        GeneralPurposeHand = 35,

        [Display(Name = "Trainee")]
        Trainee = 36,

        [Display(Name = "Other")]
        Other = 37
    }

    public enum CourseVersionStatus
    {
        [Display(Name = "Draft")]
        Draft = 0,

        [Display(Name = "Published")]
        Published = 1,

        [Display(Name = "Archived")]
        Archived = 2
    }

    public enum LessonBlockType
    {
        [Display(Name = "Text / Narrative")]
        TextNarrative = 0,

        [Display(Name = "Download")]
        Download = 6,

        [Display(Name = "Video")]
        Video = 12,

        [Display(Name = "Flashcard")]
        Flashcard = 13,

        [Display(Name = "Card")]
        Card = 14,

        [Display(Name = "Quiz")]
        Quiz = 17,

        [Display(Name = "Assessment")]
        Assessment = 18
    }

    public enum LessonCompletionRule
    {
        [Display(Name = "Manual Button")]
        ManualButton = 0,

        [Display(Name = "Automatic")]
        Automatic = 1
    }

    public enum TrainingAudienceType
    {
        [Display(Name = "All")]
        All = 0,

        [Display(Name = "Crew")]
        Crew = 1,

        [Display(Name = "Officers")]
        Officers = 2,

        [Display(Name = "Responders")]
        Responders = 3
    }

    public enum QuestionType
    {
        [Display(Name = "Single Choice")]
        SingleChoice = 0,

        [Display(Name = "Multiple Choice")]
        MultipleChoice = 1,

        [Display(Name = "True / False")]
        TrueFalse = 2,

        [Display(Name = "Free Text")]
        FreeText = 3,

        [Display(Name = "Scenario")]
        Scenario = 4
    }

    public enum ProgressStatus
    {
        [Display(Name = "Not Started")]
        NotStarted = 0,

        [Display(Name = "Started")]
        Started = 1,

        [Display(Name = "Completed")]
        Completed = 2,

        [Display(Name = "Expired")]
        Expired = 3,

        [Display(Name = "Failed")]
        Failed = 4
    }

    public enum CourseAudienceRuleType
    {
        [Display(Name = "All Crew")]
        AllCrew = 0,

        [Display(Name = "Application Role")]
        ApplicationRole = 1,

        [Display(Name = "Onboard Role")]
        OnBoardRole = 2,

        [Display(Name = "Qualification")]
        Qualification = 3,

        [Display(Name = "Manual Assignment")]
        ManualAssignment = 4
    }



    public enum TrainingQuestionType
    {
        [Display(Name = "Multiple Choice")]
        MultipleChoice = 0,

        [Display(Name = "True / False")]
        TrueFalse = 1,

        [Display(Name = "Scenario")]
        Scenario = 2
    }

    public enum TrainingProgressStatus
    {
        [Display(Name = "Not Started")]
        NotStarted = 0,

        [Display(Name = "Started")]
        Started = 1,

        [Display(Name = "Completed")]
        Completed = 2,

        [Display(Name = "Failed")]
        Failed = 3,

        [Display(Name = "Expired")]
        Expired = 4
    }

    public enum TrainingAudienceRuleType
    {
        [Display(Name = "All Crew")]
        AllCrew = 0,

        [Display(Name = "Application Role")]
        AppRole = 1,

        [Display(Name = "Onboard Role")]
        OnBoardRole = 2,

        [Display(Name = "Qualification")]
        Qualification = 3,

        [Display(Name = "Manual Assignment")]
        ManualAssignment = 4
    }

    public enum AssignmentStatus
    {
        [Display(Name = "Assigned")]
        Assigned = 0,

        [Display(Name = "Started")]
        Started = 1,

        [Display(Name = "Completed")]
        Completed = 2,

        [Display(Name = "Expired")]
        Expired = 3,

        [Display(Name = "Cancelled")]
        Cancelled = 4
    }
}
