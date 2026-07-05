using System.Text.RegularExpressions;

namespace PremiumPlace_API.Data
{
    /// <summary>
    /// Central place for normalizing city names so the same city never gets
    /// stored twice under different casing or spacing.
    /// </summary>
    public static class CityNames
    {
        /// <summary>Trims and collapses internal whitespace, preserving the original casing.</summary>
        public static string Normalize(string? name)
            => string.IsNullOrWhiteSpace(name)
                ? string.Empty
                : Regex.Replace(name.Trim(), @"\s+", " ");

        /// <summary>Case-insensitive key used to detect duplicates.</summary>
        public static string Key(string? name)
            => Normalize(name).ToLowerInvariant();
    }
}
