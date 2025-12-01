namespace Fanex.Affiliate.Link.Services.Impl
{
    using Fanex.Affiliate.Link.Constants;
    using Fanex.Affiliate.Link.DTOs;
    using Fanex.Affiliate.Link.Services.Interface;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Service for HTTP context-dependent operations.
    /// </summary>
    public class HttpContextService : IHttpContextService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Contains all device mapping.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> _deviceMapping = new Dictionary<string, string>
        {
            { "iphone", "iPhone" },
            { "ipad", "iPad" },
            { "ipod", "iPod" },
            { "blackberry", "BlackBerry" },
            { "htc", "HTC" },
            { "motorola", "Motorola" },
            { "nokia", "Nokia" }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor.</param>
        public HttpContextService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Get Country from the HTTP request.
        /// </summary>
        /// <returns>Country English Name.</returns>
        public string GetCountry()
        {
            var request = httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return "Unknown";
            }

            try
            {
                var regionInfo = ResolveCountry(request);
                if (regionInfo != null)
                {
                    return regionInfo.EnglishName;
                }
            }
            catch
            {
                // ignore and fallback
            }

            try
            {
                return new RegionInfo(CultureInfo.CurrentCulture.Name).EnglishName;
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get original protocol from the request, considering Cloudflare and proxy headers.
        /// </summary>
        /// <returns>The protocol (scheme) as a string, e.g., "http" or "https".</returns>
        public string GetOriginalProtocol()
        {
            var request = httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return "http";
            }

            // 1. Check CF-Visitor header (Cloudflare)
            var cfScheme = GetSchemeFromCfVisitor(request);
            if (!string.IsNullOrEmpty(cfScheme))
            {
                return cfScheme;
            }

            // 2. Check X-Forwarded-Proto header (may be comma-separated)
            var xForwardedScheme = GetSchemeFromXForwardedProto(request);
            if (!string.IsNullOrEmpty(xForwardedScheme))
            {
                return xForwardedScheme;
            }

            // 3. Fallback: use the request's scheme (e.g., "http" or "https")
            return request.Scheme;
        }

        private static string GetSchemeFromCfVisitor(HttpRequest request)
        {
            var cfVisitor = request.Headers[HeaderKey.CF_VISITOR].FirstOrDefault();
            if (string.IsNullOrEmpty(cfVisitor))
            {
                return null;
            }

            try
            {
                var cfVisitorInfo = JsonConvert.DeserializeObject<CfVisitorInfo>(cfVisitor);
                if (!string.IsNullOrEmpty(cfVisitorInfo?.Scheme))
                {
                    var scheme = cfVisitorInfo.Scheme.ToLowerInvariant();
                    if (scheme == "http" || scheme == "https")
                    {
                        return scheme;
                    }
                }
            }
            catch
            {
                // Ignore JSON parse errors and continue
            }

            return null;
        }

        private static string GetSchemeFromXForwardedProto(HttpRequest request)
        {
            var xForwardedProto = request.Headers[HeaderKey.X_FORWARDED_PROTO].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedProto))
            {
                var protos = xForwardedProto.Split(',');
                var proto = protos[0].Trim().ToLowerInvariant();
                if (proto == "http" || proto == "https")
                {
                    return proto;
                }
            }

            return null;
        }

        /// <summary>
        /// Get device info from user agent and HTTP request.
        /// </summary>
        /// <param name="userAgent">User's info.</param>
        /// <returns>Device info.</returns>
        public string GetDeviceName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return @"Computer/Laptop";
            }

            var deviceKey = _deviceMapping.Keys.FirstOrDefault(key =>
                userAgent.ToLower(CultureInfo.InvariantCulture).Contains(key, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(deviceKey))
            {
                return IsMobile() ? "Mobile" : @"Computer/Laptop";
            }

            return _deviceMapping[deviceKey];
        }

        /// <summary>
        /// Determines if the current request is from a mobile device.
        /// Enhanced for ASP.NET Core compatibility and robustness.
        /// </summary>
        /// <returns>True if mobile device, otherwise false.</returns>
        public bool IsMobile()
        {
            var request = httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return false;
            }

            // Try to get the User-Agent header
            var userAgent = request.Headers.UserAgent.FirstOrDefault();
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            // Use regex patterns to detect mobile devices
            // These patterns are based on common mobile user agent substrings
            const int ValidLength = 4;
            if (userAgent.Length < ValidLength)
            {
                return false;
            }

            var b = new Regex(
                @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|" +
                @"ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|" +
                @"phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|" +
                @"windows ce|xda|xiino",
                RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
            var v = new Regex(
                @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|" +
                @"amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|" +
                @"bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|" +
                @"da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|" +
                @"esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|" +
                @"haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|" +
                @"i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|" +
                @"jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|" +
                @"libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|" +
                @"mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|" +
                @"n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|" +
                @"owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|" +
                @"psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|" +
                @"sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|" +
                @"sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|" +
                @"ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|" +
                @"utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|" +
                @"w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-",
                RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));

            // Check for mobile device patterns
            if (b.IsMatch(userAgent) || v.IsMatch(userAgent.Substring(0, ValidLength)))
            {
                return true;
            }

            // Additional check: common mobile keywords
            var mobileKeywords = new[] { "mobile", "android", "iphone", "ipad", "ipod", "opera mini", "iemobile", "blackberry", "webos", "windows phone" };
            var uaLower = userAgent.ToLowerInvariant();
            if (mobileKeywords.ToList().Exists(keyword => uaLower.Contains(keyword)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get region info.
        /// </summary>
        /// <returns>Region info.</returns>
        private static RegionInfo ResolveCountry(HttpRequest request)
        {
            var culture = ResolveCulture(request);

            // Culture constants
            const int InvariantCultureLCID = 0x7F;

            //solving a culture issue
            if (culture != null && culture.LCID != InvariantCultureLCID)
            {
                return new RegionInfo(culture.Name);
            }
            return null;
        }

        /// <summary>
        /// Get culture info from the request's Accept-Language header (for .NET Core).
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>Culture info, or null if not available.</returns>
        private static CultureInfo ResolveCulture(HttpRequest request)
        {
            // In .NET Core, use Accept-Language header instead of UserLanguages
            var acceptLanguage = request.Headers["Accept-Language"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(acceptLanguage))
            {
                return null;
            }

            // Accept-Language can be a comma-separated list, e.g. "en-US,en;q=0.9,vi;q=0.8"
            var languages = acceptLanguage.Split(',')
                .Select(l => l.Split(';')[0].Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            if (languages.Length == 0)
            {
                return null;
            }

            try
            {
                var language = languages[0].ToLowerInvariant();
                return CultureInfo.CreateSpecificCulture(language);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }
    }
}