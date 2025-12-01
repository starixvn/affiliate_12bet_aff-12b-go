namespace Fanex.Affiliate.Link.Core;

public static class Constants
{
    // RabbitMQ Queue Constants
    public const string QueueIdentify = "12b.user.click";
    public const string CacheRedisQueueName = "Aff.12B.Go.ClickQueues";
    public const string TurnOnErrorQueueConsumer = "Aff.12B.Go.TurnOnErrorQueueConsumer";

    public const string MainExchangeName = $"{QueueIdentify}.exchange";
    public const string RetryExchangeName = $"{QueueIdentify}.retry.exchange";
    public const string ErrorExchangeName = $"{QueueIdentify}.error.exchange";
    public const string MainQueueName = $"{QueueIdentify}.queue";
    public const string RetryQueueName = $"{QueueIdentify}.retry.queue";
    public const string ErrorQueueName = $"{QueueIdentify}.error.queue";
    public const string RoutingKey = $"{QueueIdentify}.routing";
}

/// <summary>
/// Constants and patterns used for bot detection
/// </summary>
public static class BotDetectionConstants
{
    /// <summary>
    /// Primary threshold for bot score (used with HTTP context analysis)
    /// </summary>
    public const int PrimaryBotScoreThreshold = 60;

    /// <summary>
    /// Secondary threshold for bot score (used for user-agent only analysis)
    /// </summary>
    public const int SecondaryBotScoreThreshold = 40;

    /// <summary>
    /// Browser signatures that indicate real user browsers
    /// </summary>
    public static readonly string[] BrowserSignatures = new[]
    {
        "Windows",
        "Macintosh",
        "Linux",
        "Chrome",
        "Firefox",
        "Safari",
        "AppleWebKit",
        "Gecko"
    };

    /// <summary>
    /// Messaging apps that require smart detection (context-aware)
    /// These are handled by DetectMessagingAppBot with detailed analysis
    /// </summary>
    public static readonly string[] SpecialMessagingApps = new[]
    {
        "whatsapp",
        "instagram",
        "facebook",
        "telegram",
        "dalvik"
    };

    /// <summary>
    /// Social media and messaging preview bots (beneficial for link previews)
    /// NOTE: Instagram and Facebook are NOT here - they need smart detection
    /// to distinguish between preview bots and in-app browsers
    /// </summary>
    public static readonly string[] SocialPreviewBots = new[]
    {
        // Facebook/Instagram handled by DetectMessagingAppBot - DO NOT add here
        // They need smart detection to distinguish preview bot vs in-app browser
        
        "Twitterbot",                // Twitter/X previews
        "Slackbot",                  // Slack previews
        "Slackbot-LinkExpanding",    // Slack link expanding
        "Discordbot",                // Discord previews
        "LinkedInBot",               // LinkedIn previews
        "Pinterestbot",              // Pinterest
        "SkypeUriPreview",           // Skype previews
        "Viberbot",                  // Viber
        "Line/",                     // Line messenger
        "KakaoTalk",                 // KakaoTalk
        "ZaloPreviewBot",            // Zalo
        "SnapchatAds",               // Snapchat
        "TikTokBot",                 // TikTok
        "redditbot",                 // Reddit
        "tumblr",                    // Tumblr
        "Applebot-Extended"          // Apple Messages previews
    };

    /// <summary>
    /// Android app indicators (used by many legitimate Android apps)
    /// Requires context-aware detection to avoid false positives
    /// </summary>
    public static readonly string[] AndroidAppIndicators = new[]
    {
        "Dalvik/",                   // Android runtime (used by MANY apps)
        "okhttp/",                   // Popular Android HTTP library
        "Apache-HttpClient/",        // Used by Android apps
        "Android"                    // Generic Android
    };

    /// <summary>
    /// Malicious/Scraper bot indicators (should be excluded)
    /// Search engine crawlers, scrapers, monitoring tools, etc.
    /// </summary>
    public static readonly string[] MaliciousBotIndicators = new[]
    {
        // Search Engine Crawlers
        "Googlebot",
        "Google-InspectionTool",
        "AdsBot",
        "Mediapartners-Google",
        "bingbot",
        "AdIdxBot",
        "MSNBot",
        "Slurp",
        "YandexBot",
        "Baiduspider",
        "Applebot",                  // Apple search bot (not previews)
        "duckduckbot",
        "sogou",
        
        // Short/Generic Bots
        "zbot",
        "bitlybot",
        "CCBot",
        "crawler",
        "spider",
        "scrapy",
        "bot",
        
        // Programming Libraries/HTTP Clients
        "python-requests",
        "axios",
        "Go-http-client",
        "GuzzleHttp",
        "Java-http-client",
        "node-fetch",
        "httpclient",
        "wget",
        "curl",
        "aiohttp",
        "urllib",
        "libwww",
        
        // CMS/Frameworks
        "WordPress",
        "ELinks",
        ".NET Framework",
        
        // Monitoring/Analytics
        "feedfetcher",
        "bingpreview",
        "lighthouse",
        "gtmetrix",
        "uptimerobot",
        "feedly",
        
        // SEO/Marketing Tools
        "ahrefs",
        "semrush",
        "mj12bot",
        "petalbot",
        "censysinspect",
        "zoominfo",
        "similarweb",
        "netcraft",
        "linkfluence",
        
        // Other
        "Owler"
    };

    /// <summary>
    /// Score values for different bot detection scenarios
    /// </summary>
    public static class Scores
    {
        public const int MissingUserAgent = 50;
        public const int SuspiciousSafari = 30;
        public const int FakeSafari = 45;
        public const int SocialPreviewBot = 40;
        public const int AndroidApp = 35;
        public const int MaliciousBot = 40;
        public const int ThirdPartyDetection = 100;

        // HTTP Header penalties
        public const int MissingSecurityHeaders = 25;
        public const int WeirdAcceptHeader = 20;
        public const int NoRefererOrOrigin = 10;
        public const int NoLanguageHeader = 10;
    }
}
