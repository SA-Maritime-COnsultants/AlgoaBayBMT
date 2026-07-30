using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AlgoaBayBMT.Shared.Models
{
    public sealed record CrewRankOption(CrewRank Value, string Text);
    public sealed record SeagoingCommercialQualificationOption(SeagoingCommercialQualification Value, string Text);
    public sealed record OnBoardRoleOption(OnBoardRoles Value, string Text);

    public static class CrewRankExtensions
    {
        public static readonly IReadOnlyList<CrewRankOption> All = Enum
            .GetValues<CrewRank>()
            .Select(rank => new CrewRankOption(rank, rank.GetDisplayName()))
            .ToList();

        public static string GetDisplayName(this CrewRank rank)
        {
            var member = typeof(CrewRank).GetMember(rank.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? rank.ToString();
        }

        public static CrewRank? ParseDisplayName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            foreach (var rank in Enum.GetValues<CrewRank>())
            {
                if (string.Equals(rank.ToString(), value, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(rank.GetDisplayName(), value, StringComparison.OrdinalIgnoreCase))
                {
                    return rank;
                }
            }

            return null;
        }

        public static string GetDisplayName(this CrewRank? rank) => rank.HasValue ? rank.Value.GetDisplayName() : "Not captured";

        public static IReadOnlyList<CrewRankOption> GetOptions() => All;
    }

    public static class SeagoingCommercialQualificationExtensions
    {
        public static readonly IReadOnlyList<SeagoingCommercialQualificationOption> All = Enum
            .GetValues<SeagoingCommercialQualification>()
            .Select(value => new SeagoingCommercialQualificationOption(value, value.GetDisplayName()))
            .ToList();

        public static string GetDisplayName(this SeagoingCommercialQualification qualification)
        {
            var member = typeof(SeagoingCommercialQualification).GetMember(qualification.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? qualification.ToString();
        }

        public static string GetDisplayName(this SeagoingCommercialQualification? qualification) =>
            qualification.HasValue ? qualification.Value.GetDisplayName() : "Not captured";

        public static CrewRank ToCrewRank(this SeagoingCommercialQualification qualification) => qualification switch
        {
            SeagoingCommercialQualification.MasterForeignGoing => CrewRank.Master,
            SeagoingCommercialQualification.ChiefOfficerForeignGoing => CrewRank.ChiefOfficer,
            SeagoingCommercialQualification.OfficerInChargeNavigationalWatch => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.AbleSeafarerDeck => CrewRank.AbleSeaman,
            SeagoingCommercialQualification.RatingFormingPartNavigationalWatch => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.ChiefEngineerOfficer => CrewRank.ChiefEngineer,
            SeagoingCommercialQualification.SecondEngineerOfficer => CrewRank.SecondEngineer,
            SeagoingCommercialQualification.OfficerInChargeEngineeringWatch => CrewRank.ThirdEngineer,
            SeagoingCommercialQualification.ElectroTechnicalOfficer => CrewRank.ThirdEngineer,
            SeagoingCommercialQualification.ElectroTechnicalRating => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.AbleSeafarerEngine => CrewRank.AbleSeaman,
            SeagoingCommercialQualification.RatingFormingPartEngineeringWatch => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.BosunQualification => CrewRank.Bosun,
            SeagoingCommercialQualification.PumpmanQualification => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.DeckCadetQualification => CrewRank.DeckCadet,
            SeagoingCommercialQualification.EngineCadetQualification => CrewRank.EngineCadet,
            SeagoingCommercialQualification.GeneralPurposeRating => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.CateringRating => CrewRank.Steward,
            SeagoingCommercialQualification.OrdinarySeaman => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.Wiper => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.Motorman => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.Fitter => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.RefrigerationEngineer => CrewRank.ThirdEngineer,
            SeagoingCommercialQualification.Electrician => CrewRank.ThirdEngineer,
            SeagoingCommercialQualification.CraneOperator => CrewRank.OrdinarySeaman,
            SeagoingCommercialQualification.PumpmanTanker => CrewRank.OrdinarySeaman,
            _ => CrewRank.OrdinarySeaman
        };

        public static SeagoingCommercialQualification? ToQualification(this CrewRank? rank) =>
            rank.HasValue ? rank.Value.ToQualification() : null;

        public static SeagoingCommercialQualification ToQualification(this CrewRank rank) => rank switch
        {
            CrewRank.Captain => SeagoingCommercialQualification.MasterForeignGoing,
            CrewRank.Master => SeagoingCommercialQualification.MasterForeignGoing,
            CrewRank.ChiefOfficer => SeagoingCommercialQualification.ChiefOfficerForeignGoing,
            CrewRank.ChiefEngineer => SeagoingCommercialQualification.ChiefEngineerOfficer,
            CrewRank.SecondEngineer => SeagoingCommercialQualification.SecondEngineerOfficer,
            CrewRank.ThirdEngineer => SeagoingCommercialQualification.OfficerInChargeEngineeringWatch,
            CrewRank.Bosun => SeagoingCommercialQualification.BosunQualification,
            CrewRank.AbleSeaman => SeagoingCommercialQualification.AbleSeafarerDeck,
            CrewRank.OrdinarySeaman => SeagoingCommercialQualification.OrdinarySeaman,
            CrewRank.DeckCadet => SeagoingCommercialQualification.DeckCadetQualification,
            CrewRank.EngineCadet => SeagoingCommercialQualification.EngineCadetQualification,
            CrewRank.Cook => SeagoingCommercialQualification.CateringRating,
            CrewRank.Steward => SeagoingCommercialQualification.CateringRating,
            _ => SeagoingCommercialQualification.Other
        };

        public static IReadOnlyList<SeagoingCommercialQualificationOption> GetOptions() => All;
    }

    public static class OnBoardRolesExtensions
    {
        public static readonly IReadOnlyList<OnBoardRoleOption> All = Enum
            .GetValues<OnBoardRoles>()
            .Select(value => new OnBoardRoleOption(value, value.GetDisplayName()))
            .ToList();

        public static string GetDisplayName(this OnBoardRoles role)
        {
            var member = typeof(OnBoardRoles).GetMember(role.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? role.ToString();
        }

        public static string GetDisplayName(this OnBoardRoles? role) =>
            role.HasValue ? role.Value.GetDisplayName() : "Not captured";

        public static VesselRoleType ToVesselRoleType(this OnBoardRoles role) => role switch
        {
            OnBoardRoles.Captain => VesselRoleType.Master,
            OnBoardRoles.ChiefOfficer => VesselRoleType.ChiefOfficer,
            OnBoardRoles.SecondOfficer or OnBoardRoles.ThirdOfficer or OnBoardRoles.JuniorOfficer
                or OnBoardRoles.ChiefEngineer or OnBoardRoles.SecondEngineer or OnBoardRoles.ThirdEngineer
                or OnBoardRoles.FourthEngineer or OnBoardRoles.JuniorEngineer
                or OnBoardRoles.ElectroTechnicalOfficer or OnBoardRoles.CargoOfficer
                or OnBoardRoles.SafetyOfficer or OnBoardRoles.SecurityOfficer
                or OnBoardRoles.EnvironmentalOfficer => VesselRoleType.Officer,
            OnBoardRoles.Poac => VesselRoleType.Poac,
            _ => VesselRoleType.Crew
        };

        public static IReadOnlyList<OnBoardRoleOption> GetOptions() => All;

        private static readonly Dictionary<OnBoardRoles, int> _seniorityMap = new()
        {
            [OnBoardRoles.Captain]                = 1,
            [OnBoardRoles.CargoOfficer]           = 2,
            [OnBoardRoles.ChiefOfficer]           = 3,
            [OnBoardRoles.SecondOfficer]          = 4,
            [OnBoardRoles.ThirdOfficer]           = 5,
            [OnBoardRoles.JuniorOfficer]          = 6,
            [OnBoardRoles.DeckCadet]              = 7,
            [OnBoardRoles.ChiefEngineer]          = 8,
            [OnBoardRoles.SecondEngineer]         = 9,
            [OnBoardRoles.ThirdEngineer]          = 10,
            [OnBoardRoles.FourthEngineer]         = 11,
            [OnBoardRoles.JuniorEngineer]         = 12,
            [OnBoardRoles.EngineCadet]            = 13,
            [OnBoardRoles.ElectroTechnicalOfficer]= 14,
            [OnBoardRoles.Electrician]            = 15,
            [OnBoardRoles.RefrigerationEngineer]  = 16,
            [OnBoardRoles.Bosun]                  = 17,
            [OnBoardRoles.Pumpman]                = 18,
            [OnBoardRoles.AbleBodiedSeaman]       = 19,
            [OnBoardRoles.OrdinarySeaman]         = 20,
            [OnBoardRoles.DeckRating]             = 21,
            [OnBoardRoles.Motorman]               = 22,
            [OnBoardRoles.Oiler]                  = 23,
            [OnBoardRoles.Fitter]                 = 24,
            [OnBoardRoles.Wiper]                  = 25,
            [OnBoardRoles.ChiefCook]              = 26,
            [OnBoardRoles.Cook]                   = 27,
            [OnBoardRoles.Steward]                = 28,
            [OnBoardRoles.Messman]                = 29,
            [OnBoardRoles.GeneralPurposeHand]     = 30,
            [OnBoardRoles.CraneOperator]          = 31,
            [OnBoardRoles.SafetyOfficer]          = 32,
            [OnBoardRoles.SecurityOfficer]        = 33,
            [OnBoardRoles.EnvironmentalOfficer]   = 34,
            [OnBoardRoles.Medic]                  = 35,
            [OnBoardRoles.Poac]                   = 36,
            [OnBoardRoles.Trainee]                = 37,
            [OnBoardRoles.Other]                  = 38,
        };

        public static int GetSeniorityOrder(OnBoardRoles role) =>
            _seniorityMap.TryGetValue(role, out var order) ? order : 99;

        public static int GetSeniorityOrderByName(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return 99;
            }

            foreach (var role in Enum.GetValues<OnBoardRoles>())
            {
                if (string.Equals(role.GetDisplayName(), roleName, StringComparison.OrdinalIgnoreCase))
                {
                    return GetSeniorityOrder(role);
                }
            }

            return 99;
        }
    }
}
