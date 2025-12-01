using DeviceDetectorNET;
using Fanex.Affiliate.Link.Commands;
using Fanex.Affiliate.Link.Core.Helpers;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Affiliate.Link.Criterias;
using Fanex.Data.Repository;
using Microsoft.AspNetCore.Http;
using NetCrawlerDetect;
using NetCrawlerDetect.Fixtures;

namespace Fanex.Affiliate.Link.Core.Services;

public sealed class BotDetectionService(
    IDynamicRepository dynamicRepository,
    ILogService logService
    ) : IBotDetectionService
{
    // Use centralized constants for better maintainability and reusability
    private const int thresholdBotScore = BotDetectionConstants.PrimaryBotScoreThreshold;
    private const int thresholdBotScore2 = BotDetectionConstants.SecondaryBotScoreThreshold;

    private BotScore GetBotScore(HttpContext context)
    {
        var headers = context.Request.Headers;
        string userAgent = headers.UserAgent.FirstOrDefault() ?? string.Empty;

        // CRITICAL: Use smart detection for messaging apps first!
        var messagingBot = DetectMessagingAppBot(userAgent);
        BotScore botScore = messagingBot.isBot ? new BotScore
        {
            IsBot = true,
            Score = BotDetectionConstants.Scores.SocialPreviewBot,
            Reason = $"[{messagingBot.botType}: {messagingBot.details}]"
        } : CheckBotByUserAgent(userAgent);

        // Add context from HTTP headers (not available in user-agent alone)
        CheckSecurityHeaders(headers, botScore);
        CheckAcceptHeader(headers, botScore);
        CheckRefererAndOrigin(headers, botScore);
        CheckLanguageHeader(headers, botScore);

        string clientIp = HttpContextHelper.GetClientIpAddress(context);
        LogBotDetectionResult(botScore, clientIp, userAgent);

        return botScore;
    }

    private static void CheckSecurityHeaders(IHeaderDictionary headers, BotScore botScore)
    {
        bool hasSecHeaders = headers.ContainsKey("Sec-CH-UA") || headers.ContainsKey("Sec-Fetch-Site");
        if (!hasSecHeaders)
        {
            botScore.Score += BotDetectionConstants.Scores.MissingSecurityHeaders;
            botScore.Reason += "[Missing Sec-*] ";
        }
    }

    private static void CheckAcceptHeader(IHeaderDictionary headers, BotScore botScore)
    {
        string accept = headers.Accept.FirstOrDefault() ?? "";
        if (!accept.Contains("text/html") && !accept.Contains("application/json"))
        {
            botScore.Score += BotDetectionConstants.Scores.WeirdAcceptHeader;
            botScore.Reason += "[Weird Accept] ";
        }
    }

    private static void CheckRefererAndOrigin(IHeaderDictionary headers, BotScore botScore)
    {
        string referer = headers.Referer.FirstOrDefault() ?? "";
        string origin = headers.Origin.FirstOrDefault() ?? "";
        if (string.IsNullOrWhiteSpace(referer) && string.IsNullOrWhiteSpace(origin))
        {
            botScore.Score += BotDetectionConstants.Scores.NoRefererOrOrigin;
            botScore.Reason += "[No Referer/Origin] ";
        }
    }

    private static void CheckLanguageHeader(IHeaderDictionary headers, BotScore botScore)
    {
        string lang = headers["Accept-Language"].FirstOrDefault() ?? "";
        if (string.IsNullOrWhiteSpace(lang))
        {
            botScore.Score += BotDetectionConstants.Scores.NoLanguageHeader;
            botScore.Reason += "[No Language] ";
        }
    }

    private void LogBotDetectionResult(BotScore botScore, string clientIp, string userAgent)
    {
        if (botScore.Score >= thresholdBotScore)
        {
            botScore.IsBot = true;
            string level = botScore.Score >= 80 ? "HIGH" : "MEDIUM";
            logService.LogFileMessage($"Bot detected ({level}): Score={botScore}, Reason={botScore.Reason.Trim()}, IP={clientIp}, UA={userAgent}");
        }
    }


    public bool IsBot(HttpContext context)
    {
        var botScore = GetBotScore(context);
        if (botScore.IsBot)
        {
            return true;
        }

        string userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty;

        // Run third-party detection only if not a messaging app
        if (ShouldRunThirdPartyDetection(userAgent))
        {
            return DetectBotByThirdParty(userAgent);
        }

        return false;
    }

    public bool DetectBotFromUserAgent(string userAgent)
    {
        // CRITICAL: Use smart detection for messaging apps first!
        var messagingBot = DetectMessagingAppBot(userAgent);
        if (messagingBot.isBot)
        {
            return true;
        }

        // Then use pattern-based detection
        var botScore = CheckBotByUserAgent(userAgent);
        if (botScore.Score >= thresholdBotScore2)
        {
            return true;
        }

        // Run third-party detection only if not a messaging app
        if (ShouldRunThirdPartyDetection(userAgent))
        {
            return DetectBotByThirdParty(userAgent);
        }

        return false;
    }

    public BotScore DetectBotFromUserAgentWithDetails(string userAgent)
    {
        // Best Practice: Check for messaging/social app bots FIRST with detailed detection
        var messagingBot = DetectMessagingAppBot(userAgent);
        if (messagingBot.isBot)
        {
            return new BotScore
            {
                IsBot = true,
                Score = BotDetectionConstants.Scores.SocialPreviewBot, // Social bots get lower score (they're beneficial)
                Reason = $"[{messagingBot.botType}: {messagingBot.details}]"
            };
        }

        // Then check using standard detection
        var botScore = CheckBotByUserAgent(userAgent);

        if (botScore.Score >= thresholdBotScore2)
        {
            botScore.IsBot = true;
            if (string.IsNullOrEmpty(botScore.Reason))
            {
                botScore.Reason = "[Bot Score Threshold Exceeded]";
            }
            return botScore;
        }

        // IMPORTANT: Skip third-party detection for messaging apps we've already analyzed
        // Third-party libraries don't have our context-aware logic (WhatsApp/2 vs WhatsApp/2.24.x, etc.)
        if (ShouldRunThirdPartyDetection(userAgent))
        {
            // Finally check third-party libraries
            var thirdPartyDetection = DetectBotByThirdPartyWithDetails(userAgent);
            if (thirdPartyDetection.isBot)
            {
                botScore.IsBot = true;
                botScore.Score += BotDetectionConstants.Scores.ThirdPartyDetection;
                botScore.Reason += thirdPartyDetection.reason;
            }
        }

        return botScore;
    }

    /// <summary>
    /// Enhanced Safari detection - distinguishes between legitimate Safari and fake/suspicious patterns
    /// </summary>
    private static (bool isSuspicious, int score, string reason) DetectSuspiciousSafari(string userAgent)
    {
        if (!IsSafariUserAgent(userAgent) || IsChromiumBasedBrowser(userAgent))
        {
            return (false, 0, string.Empty);
        }

        var hasAppleWebKit = HasAppleWebKit(userAgent);
        var hasApplePlatform = HasApplePlatform(userAgent);
        var majorVersion = ExtractSafariMajorVersion(userAgent);

        // Check for suspicious version
        var versionCheck = CheckSafariVersion(majorVersion, hasAppleWebKit, hasApplePlatform, userAgent);
        if (versionCheck.isSuspicious)
        {
            return versionCheck;
        }

        // Check for missing critical components
        if (!hasAppleWebKit)
        {
            return (true, 45, "[Fake Safari: Missing AppleWebKit]");
        }

        if (!hasApplePlatform)
        {
            return (true, 40, "[Suspicious Safari: Not on Apple platform]");
        }

        // Check for missing version on desktop
        if (!HasVersionString(userAgent) && !IsMobileSafari(userAgent))
        {
            return (true, 30, "[Suspicious Safari: Missing Version string on non-mobile]");
        }

        return (false, 0, string.Empty);
    }

    private static bool IsSafariUserAgent(string userAgent)
    {
        return userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsChromiumBasedBrowser(string userAgent)
    {
        // Split into two checks to comply with max 3 conditional operators rule
        var isChrome = userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase)
                       || userAgent.Contains("Chromium", StringComparison.OrdinalIgnoreCase);

        var isEdgeOrOpera = userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase)
                            || userAgent.Contains("OPR/", StringComparison.OrdinalIgnoreCase);

        return isChrome || isEdgeOrOpera;
    }

    private static bool HasAppleWebKit(string userAgent)
    {
        return userAgent.Contains("AppleWebKit", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasApplePlatform(string userAgent)
    {
        // Split into multiple checks to comply with max 3 conditional operators rule
        var hasiOSDevice = userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
                           || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase)
                           || userAgent.Contains("iPod", StringComparison.OrdinalIgnoreCase);

        var hasMacOS = userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase)
                       || userAgent.Contains("Mac OS X", StringComparison.OrdinalIgnoreCase);

        var hasWindows = userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase);

        return hasiOSDevice || hasMacOS || hasWindows;
    }

    private static int ExtractSafariMajorVersion(string userAgent)
    {
        var versionMatch = System.Text.RegularExpressions.Regex.Match(
            userAgent,
            @"Version/([0-9]+)\.?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        return versionMatch.Success && int.TryParse(versionMatch.Groups[1].Value, out int version)
            ? version
            : 0;
    }

    private static (bool isSuspicious, int score, string reason) CheckSafariVersion(
        int majorVersion,
        bool hasAppleWebKit,
        bool hasApplePlatform,
        string userAgent)
    {
        if (majorVersion == 0)
        {
            return (false, 0, string.Empty);
        }

        // Extremely old versions (2003-2008)
        if (majorVersion <= 2)
        {
            return (true, 50, $"[Fake Safari: Version/{majorVersion}.x is from 2003-2008, extremely outdated]");
        }

        // Very old versions (2008-2011)
        if (majorVersion <= 5)
        {
            // Safari for Windows ended at 5.1.7
            if (IsWindows(userAgent))
            {
                return (false, 0, string.Empty); // Allow old Safari on Windows
            }
            return (true, 40, $"[Suspicious Safari: Version/{majorVersion}.x is very old (2008-2011)]");
        }

        // Old but possible versions (2012-2015)
        if (majorVersion <= 9 && (!hasAppleWebKit || !hasApplePlatform))
        {
            return (true, 35, $"[Suspicious Safari: Version/{majorVersion}.x without proper Apple signature]");
        }

        // Modern version on Windows (Safari for Windows ended in 2012)
        if (majorVersion > 5 && IsWindows(userAgent))
        {
            return (true, 45, $"[Fake Safari: Version/{majorVersion} on Windows (Safari for Windows ended at 5.1.7 in 2012)]");
        }

        return (false, 0, string.Empty);
    }

    private static bool IsWindows(string userAgent)
    {
        return userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasVersionString(string userAgent)
    {
        return userAgent.Contains("Version/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMobileSafari(string userAgent)
    {
        return userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase);
    }

    private static BotScore CheckBotByUserAgent(string userAgent)
    {
        var botScore = new BotScore();

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return CreateBotScore(BotDetectionConstants.Scores.MissingUserAgent, "[Missing UA]");
        }

        // Check each detection type in order
        var safariCheck = CheckSafariSuspicion(userAgent);
        if (safariCheck != null)
        {
            return safariCheck;
        }

        var socialBotCheck = CheckSocialPreviewBots(userAgent);
        if (socialBotCheck != null)
        {
            return socialBotCheck;
        }

        var androidCheck = CheckAndroidApp(userAgent);
        if (androidCheck != null)
        {
            return androidCheck;
        }

        var maliciousCheck = CheckMaliciousBots(userAgent);
        if (maliciousCheck != null)
        {
            return maliciousCheck;
        }

        return botScore;
    }

    private static BotScore? CheckSafariSuspicion(string userAgent)
    {
        var safariSuspicion = DetectSuspiciousSafari(userAgent);
        return safariSuspicion.isSuspicious
            ? CreateBotScore(safariSuspicion.score, safariSuspicion.reason)
            : null;
    }

    private static BotScore? CheckSocialPreviewBots(string userAgent)
    {
        var socialBots = BotDetectionConstants.SocialPreviewBots
            .Where(indicator => userAgent.Contains(indicator, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return socialBots.Count > 0
            ? CreateBotScore(BotDetectionConstants.Scores.SocialPreviewBot, $"[Social Preview Bot: {string.Join(", ", socialBots)}]")
            : null;
    }

    private static BotScore? CheckAndroidApp(string userAgent)
    {
        var androidIndicators = BotDetectionConstants.AndroidAppIndicators
            .Where(indicator => userAgent.Contains(indicator, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (androidIndicators.Count == 0)
        {
            return null;
        }

        // Dalvik with browser indicators is likely legitimate
        if (HasRealBrowserIndicators(userAgent))
        {
            return null;
        }

        return CreateBotScore(BotDetectionConstants.Scores.AndroidApp, $"[Android App/Dalvik: {string.Join(", ", androidIndicators)}]");
    }

    private static BotScore? CheckMaliciousBots(string userAgent)
    {
        var maliciousBots = BotDetectionConstants.MaliciousBotIndicators
            .Where(indicator => userAgent.Contains(indicator, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return maliciousBots.Count > 0
            ? CreateBotScore(BotDetectionConstants.Scores.MaliciousBot, $"[Malicious/Scraper Bot: {string.Join(", ", maliciousBots)}]")
            : null;
    }

    private static BotScore CreateBotScore(int score, string reason)
    {
        return new BotScore
        {
            Score = score,
            Reason = $"{reason} "
        };
    }

    private static bool HasRealBrowserIndicators(string userAgent)
    {
        return userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase)
               || userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase)
               || userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase);
    }

    private static bool DetectBotByThirdParty(string userAgent)
    {
        // CrawlerDetect may throw only fatal errors, usually safe
        var crawlerDetect = new CrawlerDetect();
        if (crawlerDetect.IsCrawler(userAgent))
        {
            return true;
        }

        var deviceDetector = new DeviceDetector(userAgent);
        deviceDetector.Parse();
        if (deviceDetector.IsBot())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Detect bot using third-party libraries with detailed reasons
    /// Cyclomatic Complexity: 3 (reduced from 12 by extracting helper methods)
    /// </summary>
    private static (bool isBot, string reason) DetectBotByThirdPartyWithDetails(string userAgent)
    {
        // Try CrawlerDetect first
        var crawlerResult = TryDetectBotWithCrawlerDetect(userAgent);
        if (crawlerResult.isBot)
        {
            return crawlerResult;
        }

        // Try DeviceDetector if CrawlerDetect didn't find a bot
        var deviceDetectorResult = TryDetectBotWithDeviceDetector(userAgent);
        if (deviceDetectorResult.isBot)
        {
            return deviceDetectorResult;
        }

        return (false, string.Empty);
    }

    /// <summary>
    /// Try to detect bot using CrawlerDetect library
    /// Cyclomatic Complexity: 4
    /// </summary>
    private static (bool isBot, string reason) TryDetectBotWithCrawlerDetect(string userAgent)
    {
        try
        {
            var crawlerDetect = new CrawlerDetect();
            if (!crawlerDetect.IsCrawler(userAgent))
            {
                return (false, string.Empty);
            }

            // Extract which bot pattern was matched from the user agent
            var matchedPattern = ExtractMatchedCrawlerPattern(userAgent);
            if (!string.IsNullOrEmpty(matchedPattern))
            {
                return (true, $"[CrawlerDetect: matched '{matchedPattern}'] ");
            }

            return (true, "[CrawlerDetect: Bot Detected] ");
        }
        catch (Exception)
        {
            // If CrawlerDetect fails, return not a bot (will try other detectors)
            return (false, string.Empty);
        }
    }

    /// <summary>
    /// Try to detect bot using DeviceDetector library
    /// Cyclomatic Complexity: 4 (reduced from 11 by extracting helper method)
    /// </summary>
    private static (bool isBot, string reason) TryDetectBotWithDeviceDetector(string userAgent)
    {
        try
        {
            var deviceDetector = new DeviceDetector(userAgent);
            deviceDetector.Parse();

            if (!deviceDetector.IsBot())
            {
                return (false, string.Empty);
            }

            var botInfo = deviceDetector.GetBot();
            string botDetails = FormatDeviceDetectorBotDetails(botInfo);
            return (true, botDetails);
        }
        catch (Exception)
        {
            // If DeviceDetector fails, return not a bot
            return (false, string.Empty);
        }
    }

    /// <summary>
    /// Format bot details from DeviceDetector library result
    /// Cyclomatic Complexity: 5
    /// </summary>
    private static string FormatDeviceDetectorBotDetails(dynamic? botInfo)
    {
        string botDetails = "[DeviceDetector";

        if (!(botInfo?.Success ?? false) || botInfo?.Match == null)
        {
            botDetails += "] ";
            return botDetails;
        }

        // At this point, botInfo and botInfo.Match are guaranteed to be non-null
        string? botName = botInfo!.Match!.Name;
        string? botCategory = botInfo!.Match!.Category;

        if (!string.IsNullOrEmpty(botName))
        {
            botDetails += $": {botName}";
        }

        if (!string.IsNullOrEmpty(botCategory))
        {
            botDetails += $" ({botCategory})";
        }

        botDetails += "] ";
        return botDetails;
    }

    /// <summary>
    /// Best Practice: Detect specific messaging app bots with detailed patterns
    /// Cyclomatic Complexity: 3 (reduced from 14 by using loop-based approach)
    /// </summary>
    private static (bool isBot, string botType, string details) DetectMessagingAppBot(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return (false, string.Empty, string.Empty);
        }

        var ua = userAgent.ToLowerInvariant();

        // Define detectors in priority order - loop reduces complexity from 14 to 3
        var detectors = GetMessagingAppDetectors(userAgent, ua);

        foreach (var detector in detectors)
        {
            var result = detector();
            if (result.HasValue)
            {
                return result.Value;
            }
        }

        return (false, string.Empty, string.Empty);
    }

    /// <summary>
    /// Returns array of detector functions in priority order
    /// This method extracts the detector chain to reduce cyclomatic complexity
    /// </summary>
    private static Func<(bool isBot, string botType, string details)?>[] GetMessagingAppDetectors(string userAgent, string ua)
    {
        return new Func<(bool isBot, string botType, string details)?>[]
        {
            () => DetectWhatsApp(userAgent, ua),
            () => DetectInstagram(userAgent, ua),
            () => DetectFacebook(userAgent, ua),
            () => DetectTelegram(userAgent, ua),
            () => DetectDalvik(userAgent, ua),
            () => DetectTwitter(ua),
            () => DetectDiscord(ua),
            () => DetectSlack(ua),
            () => DetectLinkedIn(ua),
            () => DetectTikTok(ua),
            () => DetectLine(userAgent, ua),
            () => DetectViber(ua)
        };
    }

    private static (bool isBot, string botType, string details)? DetectWhatsApp(string userAgent, string ua)
    {
        if (!ua.Contains("whatsapp"))
        {
            return null;
        }

        // WhatsApp Web/Desktop with browser signature
        if (HasBrowserSignature(userAgent))
        {
            return (false, string.Empty, string.Empty);
        }

        // WhatsApp/2 (major version only) is Web/Desktop
        var version = ExtractVersion(userAgent, "WhatsApp");
        if (IsMajorVersionOnly(version) && version == "2")
        {
            return (false, string.Empty, string.Empty);
        }

        // Full version number = preview bot
        return (true, "WhatsApp", $"Preview bot{(string.IsNullOrEmpty(version) ? "" : $" v{version}")}");
    }

    private static (bool isBot, string botType, string details)? DetectInstagram(string userAgent, string ua)
    {
        if (!ua.Contains("instagram"))
        {
            return null;
        }

        // In-app browser has device/locale info
        if (HasInstagramDeviceInfo(userAgent) || HasLocaleInfo(userAgent))
        {
            return (false, string.Empty, string.Empty);
        }

        var version = ExtractVersion(userAgent, "Instagram");
        return (true, "Instagram", $"Preview bot{(string.IsNullOrEmpty(version) ? "" : $" v{version}")}");
    }

    private static (bool isBot, string botType, string details)? DetectFacebook(string userAgent, string ua)
    {
        // Check if it's a Facebook-related user agent
        if (!IsFacebookUserAgent(ua))
        {
            return null;
        }

        // In-app browser has FBAV/FBAN
        if (HasFacebookAppSignature(userAgent))
        {
            return (false, string.Empty, string.Empty);
        }

        var version = ExtractVersion(userAgent, "facebookexternalhit");
        return (true, "Facebook", $"Link preview bot{(string.IsNullOrEmpty(version) ? "" : $" v{version}")}");
    }

    private static bool IsFacebookUserAgent(string ua)
    {
        return ua.Contains("facebookexternalhit")
               || ua.Contains("facebot")
               || ua.Contains("facebookbot");
    }

    private static (bool isBot, string botType, string details)? DetectTelegram(string userAgent, string ua)
    {
        if (!ua.Contains("telegram"))
        {
            return null;
        }

        var version = ExtractVersion(userAgent, "Telegram");
        return (true, "Telegram", $"Bot{(string.IsNullOrEmpty(version) ? "" : $" v{version}")}");
    }

    private static (bool isBot, string botType, string details)? DetectDalvik(string userAgent, string ua)
    {
        if (!ua.Contains("dalvik/"))
        {
            return null;
        }

        // Real device = real user
        if (HasRealDeviceSignature(userAgent))
        {
            return (false, string.Empty, string.Empty);
        }

        // No device signature = bot/scraper
        var version = ExtractVersion(userAgent, "Dalvik");
        var androidVersion = ExtractVersion(userAgent, "Android");
        var details = $"Scraper/bot (Dalvik{(string.IsNullOrEmpty(version) ? "" : $" {version}")})";
        if (!string.IsNullOrEmpty(androidVersion))
        {
            details += $" on Android {androidVersion}";
        }
        return (true, "Dalvik/Android", details);
    }

    private static (bool isBot, string botType, string details)? DetectTwitter(string ua)
    {
        if (ua.Contains("twitterbot"))
        {
            return (true, "Twitter", "Link preview bot");
        }
        return null;
    }

    private static (bool isBot, string botType, string details)? DetectDiscord(string ua)
    {
        if (ua.Contains("discordbot"))
        {
            return (true, "Discord", "Embed bot");
        }
        return null;
    }

    private static (bool isBot, string botType, string details)? DetectSlack(string ua)
    {
        if (ua.Contains("slackbot"))
        {
            return (true, "Slack", "Link preview bot");
        }
        return null;
    }

    private static (bool isBot, string botType, string details)? DetectLinkedIn(string ua)
    {
        if (ua.Contains("linkedinbot"))
        {
            return (true, "LinkedIn", "Preview bot");
        }
        return null;
    }

    private static (bool isBot, string botType, string details)? DetectTikTok(string ua)
    {
        if (ua.Contains("tiktok") || ua.Contains("bytespider"))
        {
            return (true, "TikTok", "Bot");
        }
        return null;
    }

    private static (bool isBot, string botType, string details)? DetectLine(string userAgent, string ua)
    {
        if (!ua.Contains("line/"))
        {
            return null;
        }

        var version = ExtractVersion(userAgent, "Line");
        return (true, "Line", $"Messenger{(string.IsNullOrEmpty(version) ? "" : $" v{version}")}");
    }

    private static (bool isBot, string botType, string details)? DetectViber(string ua)
    {
        if (ua.Contains("viberbot") || ua.Contains("viber"))
        {
            return (true, "Viber", "Messenger bot");
        }
        return null;
    }

    // Helper methods for complex conditions
    private static bool HasBrowserSignature(string userAgent)
    {
        return Array.Exists(BotDetectionConstants.BrowserSignatures, sig => userAgent.Contains(sig, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMajorVersionOnly(string version)
    {
        return !string.IsNullOrEmpty(version) && !version.Contains(".");
    }

    private static bool HasInstagramDeviceInfo(string userAgent)
    {
        // Split into two checks to comply with max 3 conditional operators rule
        var hasiOSDevice = userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
                           || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase);

        var hasScreenInfo = userAgent.Contains("scale=", StringComparison.OrdinalIgnoreCase)
                            || System.Text.RegularExpressions.Regex.IsMatch(userAgent, @"\d{3,4}x\d{3,4}");

        return hasiOSDevice || hasScreenInfo;
    }

    private static bool HasLocaleInfo(string userAgent)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(userAgent, @"[a-z]{2}_[A-Z]{2}");
    }

    private static bool HasFacebookAppSignature(string userAgent)
    {
        return userAgent.Contains("FBAV", StringComparison.OrdinalIgnoreCase)
               || userAgent.Contains("FBAN", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasRealDeviceSignature(string userAgent)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            userAgent,
            @"(SM-[A-Z0-9]+|V\d{4}|OPPO|Redmi|Mi |OnePlus|Pixel|Huawei|Nokia|Xiaomi|Realme|iPhone\d+|iPad\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
    }

    /// <summary>
    /// Check if user agent is from a messaging/social app that we handle with smart detection
    /// </summary>
    private static bool IsMessagingAppUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return false;
        }

        var ua = userAgent.ToLowerInvariant();

        return Array.Exists(BotDetectionConstants.SpecialMessagingApps, app => ua.Contains(app, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Run third-party bot detection, skipping messaging apps (they use smart detection)
    /// </summary>
    private static bool ShouldRunThirdPartyDetection(string userAgent)
    {
        // Skip third-party detection for messaging apps
        // They're handled by our smart DetectMessagingAppBot logic
        return !IsMessagingAppUserAgent(userAgent);
    }

    /// <summary>
    /// Extract version number from user agent string
    /// </summary>
    private static string ExtractVersion(string userAgent, string appName)
    {
        try
        {
            var index = userAgent.IndexOf(appName, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return string.Empty;
            }

            var afterApp = userAgent.Substring(index + appName.Length);
            var match = System.Text.RegularExpressions.Regex.Match(afterApp, @"[\s/]?(\d+\.?\d*\.?\d*)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractMatchedCrawlerPattern(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return string.Empty;
        }

        // Common bot patterns to check (subset of what CrawlerDetect checks)
        var commonBotPatterns = new Crawlers().GetAll();

        // Find the first matching pattern (case-insensitive)
        var lowerUserAgent = userAgent.ToLowerInvariant();
        foreach (var pattern in commonBotPatterns)
        {
            var lowerPattern = pattern.ToLowerInvariant();
            int index = lowerUserAgent.IndexOf(lowerPattern, StringComparison.Ordinal);
            if (index >= 0)
            {
                // Extract the actual text from the user agent (preserving original case)
                int endIndex = Math.Min(index + pattern.Length, userAgent.Length);
                return userAgent.Substring(index, endIndex - index);
            }
        }

        return string.Empty;
    }

    public async Task DetectBot(DateTime fromDate, DateTime toDate)
    {
        var criteria = new GetClickStatisticCriteria
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        var normalClicks = new List<long>();
        var excludedClicks = new List<long>();

        var clickStatistics = await dynamicRepository.FetchAsync<ClickStatistic>(criteria).ConfigureAwait(false);
        if (clickStatistics != null)
        {
            foreach (var clickStatistic in clickStatistics)
            {
                ClassifyClick(normalClicks, excludedClicks, clickStatistic);
            }
        }

        var tasks = new List<Task>(2);

        if (normalClicks.Count > 0)
        {
            tasks.Add(UpdateClickStatisticAsync(normalClicks, isExcluded: false));
        }
        if (excludedClicks.Count > 0)
        {
            tasks.Add(UpdateClickStatisticAsync(excludedClicks, isExcluded: true));
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    private async Task UpdateClickStatisticAsync(IEnumerable<long> clickIds, bool isExcluded)
    {
        if (clickIds == null)
        {
            return;
        }

        var ids = string.Join(",", clickIds);
        if (string.IsNullOrEmpty(ids))
        {
            return;
        }

        var command = new UpdateClickStatisticCommand
        {
            ClickIDs = ids,
            IsExcluded = isExcluded,
            ErrMsg = string.Empty,
        };

        await dynamicRepository.ExecuteAsync(command).ConfigureAwait(false);
    }

    private void ClassifyClick(ICollection<long> normalClicks, ICollection<long> excludedClicks, ClickStatistic clickStatistic)
    {
        if (clickStatistic == null)
        {
            return;
        }

        var isBot = DetectBotFromUserAgent(clickStatistic.UserAgent);

        if (isBot && !clickStatistic.IsIsExcluded)
        {
            excludedClicks.Add(clickStatistic.ClickId);
        }
        else if (!isBot && clickStatistic.IsIsExcluded)
        {
            normalClicks.Add(clickStatistic.ClickId);
        }
        else
        {
            // Click state is already correct, no update needed
        }
    }
}
