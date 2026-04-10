using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AlgoaBayBMT.Shared.Models
{
    public static class CrewRankExtensions
    {
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

        public static IReadOnlyList<(CrewRank Rank, string Text)> GetOptions() =>
            Enum.GetValues<CrewRank>()
                .Select(rank => (rank, rank.GetDisplayName()))
                .ToList();
    }
}
