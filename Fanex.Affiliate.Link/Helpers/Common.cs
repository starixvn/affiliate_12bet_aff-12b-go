namespace Fanex.Affiliate.Link.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Common class.
    /// </summary>
    public static class Common
    {
        // Browser name constants
        private const string ChromeBrowser = "Chrome";
        private const string SafariBrowser = "Safari";
        private const string EdgeBrowser = "Edge";
        private const string OperaBrowser = "Opera";
        private const string FirefoxBrowser = "Firefox";
        private const string InternetExplorerBrowser = "Internet Explorer";

        /// <summary>
        /// Contains all operating system mapping.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> _operatingSystemMapping = new Dictionary<string, string>
        {
                { "Windows NT 6.1", "Windows 7" },
                { "Windows NT 6.0", "Windows Vista" },
                { "Windows NT 5.1", "Windows XP" },
                { "Windows NT 5.0", "Windows 2000" },
                { "Windows NT 4.1", "Windows 98" },
                { "iPhone", "iOS" },
                { "iPad", "iOS" },
                { "iPod", "iOS" },
                { "Mac OS", "Mac OS" },
                { "BlackBerry", "BlackBerry" },
                { "Android", "Android" },
                { "Linux", "Linux" },
                { "Ubuntu", "Ubuntu" }
        };

        /// <summary>
        /// Get OS info.
        /// </summary>
        /// <param name="userAgent">User's info.</param>
        /// <returns>Operating system info.</returns>
        public static string GetOsName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return string.Empty;
            }

            var osKey = _operatingSystemMapping.Keys.FirstOrDefault(key => userAgent.Contains(key));

            return string.IsNullOrEmpty(osKey) ? string.Empty : _operatingSystemMapping[osKey];
        }

        public static string GetDomainReferrer(string urlReferrer)
        {
            if (Uri.IsWellFormedUriString(urlReferrer, UriKind.Absolute))
            {
                var referrerUri = new Uri(urlReferrer);
                return string.IsNullOrEmpty(referrerUri.Host) ? null : referrerUri.Host;
            }

            return null;
        }

        /// <summary>
        /// Get browser name from user agent string.
        /// </summary>
        /// <param name="userAgent">User's user agent string.</param>
        /// <returns>Browser name.</returns>
        public static string GetBrowserName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return string.Empty;
            }

            userAgent = userAgent.ToLowerInvariant();

            // Define browser patterns in order of specificity
            var browserPatterns = new[]
            {
                (EdgeBrowser, new[] { "edg/", "edge/" }),
                (OperaBrowser, new[] { "opr/", "opera" }),
                (ChromeBrowser, new[] { "chrome" }),
                (SafariBrowser, new[] { "safari" }),
                (FirefoxBrowser, new[] { "firefox" }),
                (InternetExplorerBrowser, new[] { "msie", "trident" })
            };

            foreach (var (browserName, patterns) in browserPatterns)
            {
                if (patterns.ToList().Exists(pattern => userAgent.Contains(pattern)))
                {
                    // Special handling for Chrome to avoid false positives
                    if (browserName == ChromeBrowser && (userAgent.Contains("edg/") || userAgent.Contains("opr/")))
                    {
                        continue;
                    }
                    // Special handling for Safari to avoid Chrome false positives
                    if (browserName == SafariBrowser && userAgent.Contains("chrome"))
                    {
                        continue;
                    }
                    return browserName;
                }
            }

            return "Other";
        }

        /// <summary>
        /// Get browser version from user agent string.
        /// </summary>
        /// <param name="userAgent">User's user agent string.</param>
        /// <returns>Browser version.</returns>
        public static string GetBrowserVersion(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return string.Empty;
            }

            userAgent = userAgent.ToLowerInvariant();

            // Define browser version patterns in order of specificity
            var browserVersionPatterns = new[]
            {
                (EdgeBrowser, new[] { @"edg/([\d\.]+)", @"edge/([\d\.]+)" }),
                (OperaBrowser, new[] { @"opr/([\d\.]+)", @"opera/([\d\.]+)" }),
                (ChromeBrowser, new[] { @"chrome/([\d\.]+)" }),
                (SafariBrowser, new[] { @"version/([\d\.]+)" }),
                (FirefoxBrowser, new[] { @"firefox/([\d\.]+)" }),
                (InternetExplorerBrowser, new[] { @"msie ([\d\.]+)", @"rv:([\d\.]+)" })
            };

            foreach (var (browserName, patterns) in browserVersionPatterns)
            {
                var version = GetVersionForBrowser(userAgent, browserName, patterns);
                if (!string.IsNullOrEmpty(version))
                {
                    return version;
                }
            }

            return string.Empty;
        }

        private static string GetVersionForBrowser(string userAgent, string browserName, string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                var match = RegexHelper.Match(userAgent, pattern);
                if (match.Success && IsValidBrowserMatch(userAgent, browserName, pattern))
                {
                    return match.Groups[1].Value;
                }
            }
            return string.Empty;
        }

        private static bool IsValidBrowserMatch(string userAgent, string browserName, string pattern)
        {
            // Special handling for Chrome to avoid false positives
            if (browserName == ChromeBrowser && (userAgent.Contains("edg/") || userAgent.Contains("opr/")))
            {
                return false;
            }
            // Special handling for Safari to avoid Chrome false positives
            if (browserName == SafariBrowser && userAgent.Contains("chrome"))
            {
                return false;
            }
            // Special handling for IE Trident
            if (browserName == InternetExplorerBrowser && pattern == @"rv:([\d\.]+)" && !userAgent.Contains("trident"))
            {
                return false;
            }
            return true;
        }
    }
}