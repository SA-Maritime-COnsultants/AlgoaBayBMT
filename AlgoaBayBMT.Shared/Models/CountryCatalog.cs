using System.Globalization;

namespace AlgoaBayBMT.Shared.Models
{
    public static class CountryCatalog
    {
        private static readonly Lazy<IReadOnlyList<string>> Countries = new(() =>
            CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name).EnglishName;
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList()!);

        public static IReadOnlyList<string> All => Countries.Value;
    }
}
