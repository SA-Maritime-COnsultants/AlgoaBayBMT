namespace AlgoaBayBMT.Emergency.OilSpill.Models
{
    public enum OilSpillProductType
    {
        Unknown = 0,
        HFO,
        MGO,
        Crude,
        Chemicals,
        Other,
        VLSFO
    }

    public enum OilSpillSourceType
    {
        Unknown = 0,
        Barge,
        ReceivingVessel,
        Pipeline,
        Tank,
        Other
    }

    public enum OilSpillStatus
    {
        Active = 0,
        Contained,
        Closed
    }

    public enum OilSpillTideState
    {
        Unknown = 0,
        Flood,
        Ebb,
        Slack
    }

    public enum OilSpillActionType
    {
        Other = 0,
        DeployBoom,
        Skimmer,
        Dispersant,
        ShorelineProtection,
        AerialRecon
    }

    /// <summary>Supported IMS/ICS incident-management form types.</summary>
    public enum IncidentFormType
    {
        /// <summary>ICS-201 Incident Briefing.</summary>
        ICS201 = 201,

        /// <summary>ICS-204 Assignment List.</summary>
        ICS204 = 204,

        /// <summary>ICS-209 Incident Status Summary (always linked to SITREP).</summary>
        ICS209 = 209,

        /// <summary>ICS-214 Activity Log.</summary>
        ICS214 = 214
    }

    /// <summary>Lifecycle state of an incident form.</summary>
    public enum IncidentFormStatus
    {
        Draft = 0,
        Completed,
        Exported
    }
}
