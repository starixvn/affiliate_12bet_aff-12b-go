namespace Fanex.Affiliate.Link.Constants
{
    /// <summary>
    /// Provides route constraint patterns for affiliate link routing.
    /// Patterns are dynamically generated from existing constants to ensure consistency.
    /// </summary>
    public static class RouteConstraints
    {
        /// <summary>
        /// Language constraint pattern generated from Language.SupportedLanguageCodes and RemovedLanguageCodes.
        /// Includes: en, bd, bn, hien, hitg, id, jp, ko, th, vn, cs, mycs, ch, kh, eneuro, phen
        /// </summary>
        public static readonly string LanguageRegexPattern =
            @"(" + string.Join("|", Language.SupportedLanguageCodes.Keys
                .Concat(Language.RemovedLanguageCodes.Keys)
                .Distinct()
                .OrderByDescending(x => x.Length) // Longer patterns first to avoid partial matches
                ) + ")";

        /// <summary>
        /// Numeric regex pattern that matches a range of numbers (one or more digits).
        /// Used for both affiliate IDs and affiliate codes since they have the same pattern.
        /// </summary>
        public const string NumericRegexPattern = @"^[0-9]+$";

        /// <summary>
        /// Complete page code regex pattern with anchors for exact matching.
        /// </summary>
        public const string PageCodeRegexPattern = @"^[a-zA-Z0-9_-]+$";
    }
}