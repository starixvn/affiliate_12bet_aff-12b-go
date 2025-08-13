using System.Text.RegularExpressions;

namespace Fanex.Affiliate.Link.Constants
{
    /// <summary>
    /// Centralized regex patterns using GeneratedRegex for optimal performance.
    /// Used by both AffiliateErrorMiddleware and AffiliateValidationFilter to avoid duplication.
    /// </summary>
    public static partial class RegexPatterns
    {
        /// <summary>
        /// Generated regex for numeric validation (affiliate IDs and codes).
        /// Pattern: ^[0-9]+
        /// </summary>
        [GeneratedRegex(RouteConstraints.NumericRegexPattern, RegexOptions.Compiled)]
        public static partial Regex Numeric();

        /// <summary>
        /// Generated regex for page code validation.
        /// Pattern: ^[a-zA-Z0-9/_-]+$
        /// </summary>
        [GeneratedRegex(RouteConstraints.PageCodeRegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase)]
        public static partial Regex PageCode();

        /// <summary>
        /// Language regex pattern (dynamically generated from supported languages).
        /// Cannot use GeneratedRegex since the pattern is built at runtime.
        /// </summary>
        public static readonly Regex Language = new($"^{RouteConstraints.LanguageRegexPattern}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
    }
}