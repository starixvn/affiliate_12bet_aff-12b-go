using Fanex.Affiliate.Link.Constants;
using System.Text;

namespace Fanex.Affiliate.Link.Middleware
{
    /// <summary>
    /// Middleware that handles 404 responses for affiliate link patterns and provides detailed error messages.
    /// Analyzes the URL structure to determine why the route failed and gives specific feedback.
    /// </summary>
    public class AffiliateErrorMiddleware(RequestDelegate next, ILogger<AffiliateErrorMiddleware> logger)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<AffiliateErrorMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private const string Incorrect_Link_Format = "This link is in an incorrect format. Please contact administrator!";

        // HTTP Status Codes
        private const int HttpStatusNotFound = 404;
        private const int HttpStatusOk = 200;

        // Segment validation constants
        private const int MinimumSegmentCount = 3;
        private const int ThreeSegmentPattern = 3;
        private const int FourSegmentPattern = 4;
        private const int FiveSegmentPattern = 5;

        public async Task InvokeAsync(HttpContext context)
        {
            WriteLogRequest(context);
            await _next(context);

            if (ShouldHandleError(context))
            {
                var customError = AnalyzeAffiliateUrlError(context.Request.Path.Value);
                if (!string.IsNullOrEmpty(customError))
                {
                    await HandleAffiliateError(context, customError);
                }
            }
        }

        private void WriteLogRequest(HttpContext context)
        {
            try
            {
                var props = new Dictionary<string, string>();

                foreach (var header in context.Request.Headers)
                {
                    if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    props[header.Key] = header.Value.ToString();
                }

                _logger.LogInformation("Incoming request with headers: {@Headers}", props);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Write Log Request error:");
            }
        }

        private static bool ShouldHandleError(HttpContext context)
        {
            return context.Response.StatusCode == HttpStatusNotFound
                && !context.Response.HasStarted
                && !string.IsNullOrEmpty(context.Request.Path.Value)
                && context.Request.Path.Value.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase);
        }

        private async Task HandleAffiliateError(HttpContext context, string customError)
        {
            var path = context.Request.Path.Value;
            _logger.LogWarning("Affiliate URL validation failed: {Path} - {Error}", path, customError);

            // Store the error information in HttpContext.Items 
            context.Items["CustomErrorMessage"] = customError;
            context.Items["OriginalPath"] = path;

            // Generate and return the error page HTML directly
            // This mimics what the Link view would show for error cases
            await RenderAffiliateErrorPage(context, customError);
        }

        private static async Task RenderAffiliateErrorPage(HttpContext context, string customError)
        {
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                    <title>12Bet Affiliate Link</title>
                </head>
                <body>
                    <div>
                        <h2>12Bet Affiliate Link</h2>
                        <strong>This link is in an incorrect format. Please contact administrator!</strong>
                        <p>{System.Net.WebUtility.HtmlEncode(customError)}</p>
                    </div>
                </body>
                </html>";

            context.Response.Clear();
            context.Response.StatusCode = HttpStatusOk;
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(html, Encoding.UTF8);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarQube", "S3995:Change this return type to 'System.Uri'",
            Justification = "This method analyzes URL paths and returns error messages, not URLs")]
        private string AnalyzeAffiliateUrlError(string path)
        {
            try
            {
                var segments = ExtractPathSegments(path);

                if (segments.Length < MinimumSegmentCount)
                {
                    return "Invalid affiliate link format. Minimum required: pageCode/affCode/language/index.html";
                }

                return segments.Length switch
                {
                    FiveSegmentPattern => ValidateSpecialPageCodeChannelPattern(segments),
                    FourSegmentPattern => ValidateFourSegmentPattern(segments),
                    ThreeSegmentPattern => ValidateThreeSegmentPattern(segments),
                    _ => Incorrect_Link_Format
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing affiliate URL: {Path}", path);
                return Incorrect_Link_Format;
            }
        }

        private static string[] ExtractPathSegments(string path)
        {
            return path.TrimStart('/').TrimEnd('/')
                .Replace("/index.html", "", StringComparison.OrdinalIgnoreCase)
                .Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        private static string ValidateSpecialPageCodeChannelPattern(string[] segments)
        {
            // Array index constants for 5-segment pattern
            const int PageCodePart1Index = 0;
            const int PageCodePart2Index = 1;
            const int AffCodeIndex = 2;
            const int LangIndex = 3;
            const int ChannelIndex = 4;

            var (pageCodePart1, pageCodePart2, affCode, lang, _) =
                (segments[PageCodePart1Index], segments[PageCodePart2Index], segments[AffCodeIndex], segments[LangIndex], segments[ChannelIndex]);

            return ValidatePageCodePart(pageCodePart1, "part 1")
                ?? ValidatePageCodePart(pageCodePart2, "part 2")
                ?? ValidateNumeric(affCode, "affiliate code")
                ?? ValidateLanguage(lang)
                ?? $"Affiliate link format is correct, but the combination '{pageCodePart1}/{pageCodePart2}' with affiliate '{affCode}' in '{lang}' language may not be active.";
        }

        private static string ValidateFourSegmentPattern(string[] segments)
        {
            var isNumericFirstSegment = RegexPatterns.Numeric().IsMatch(segments[0]);
            return isNumericFirstSegment
                ? ValidateSpecialPageCodeOldLinkPattern(segments)
                : ValidateChannelLinkPattern(segments);
        }

        private static string ValidateChannelLinkPattern(string[] segments)
        {
            // Array index constants for 4-segment pattern
            const int PageCodeIndex = 0;
            const int AffCodeIndex = 1;
            const int LangIndex = 2;
            const int ChannelIndex = 3;

            var (pageCode, affCode, lang, _) = (segments[PageCodeIndex], segments[AffCodeIndex], segments[LangIndex], segments[ChannelIndex]);

            return ValidatePageCode(pageCode)
                ?? ValidateNumeric(affCode, "affiliate code")
                ?? ValidateLanguage(lang)
                ?? $"Affiliate link format is correct, but page '{pageCode}' with affiliate '{affCode}' may not be available in '{lang}' language.";
        }

        private static string ValidateSpecialPageCodeOldLinkPattern(string[] segments)
        {
            // Array index constants for 4-segment legacy pattern
            const int AffIdIndex = 0;
            const int PageCodePart1Index = 1;
            const int PageCodePart2Index = 2;
            const int LangIndex = 3;

            var (affId, pageCodePart1, pageCodePart2, lang) = (segments[AffIdIndex], segments[PageCodePart1Index], segments[PageCodePart2Index], segments[LangIndex]);

            return ValidateNumeric(affId, "affiliate ID")
                ?? ValidatePageCodePart(pageCodePart1, "part 1")
                ?? ValidatePageCodePart(pageCodePart2, "part 2")
                ?? ValidateLanguage(lang)
                ?? $"Legacy affiliate link format is correct, but affiliate ID '{affId}' with page '{pageCodePart1}/{pageCodePart2}' may not be active.";
        }

        private static string ValidateThreeSegmentPattern(string[] segments)
        {
            // Array index constants for 3-segment pattern
            const int FirstSegmentIndex = 0;
            const int SecondSegmentIndex = 1;
            const int ThirdSegmentIndex = 2;

            var (first, second, third) = (segments[FirstSegmentIndex], segments[SecondSegmentIndex], segments[ThirdSegmentIndex]);

            return RegexPatterns.Numeric().IsMatch(first)
                ? ValidatePageCode(second)
                    ?? ValidateLanguage(third)
                    ?? $"Legacy affiliate link format is correct, but affiliate ID '{first}' with page '{second}' may not be active."
                : ValidatePageCode(first)
                    ?? ValidateNumeric(second, "affiliate code")
                    ?? ValidateLanguage(third)
                    ?? $"Affiliate link format is correct, but page '{first}' with affiliate '{second}' may not be available in '{third}' language.";
        }

        // Validation helper methods
        private static string ValidatePageCode(string pageCode)
        {
            var isPageCodeValid = RegexPatterns.PageCode().IsMatch(pageCode);
            return !isPageCodeValid
                ? $"Invalid page code '{pageCode}'."
                : null;
        }

        private static string ValidatePageCodePart(string pageCodePart, string partName)
        {
            var isPageCodePartValid = RegexPatterns.PageCode().IsMatch(pageCodePart);
            return !isPageCodePartValid
                ? $"Invalid page code {partName} '{pageCodePart}'."
                : null;
        }

        private static string ValidateNumeric(string value, string fieldName)
        {
            var isNumericValid = RegexPatterns.Numeric().IsMatch(value);
            return !isNumericValid
                ? $"Invalid {fieldName} '{value}'."
                : null;
        }

        private static string ValidateLanguage(string lang)
        {
            var isLanguageValid = RegexPatterns.Language.IsMatch(lang);
            return !isLanguageValid ? $"Invalid language '{lang}'." : null;
        }
    }
}