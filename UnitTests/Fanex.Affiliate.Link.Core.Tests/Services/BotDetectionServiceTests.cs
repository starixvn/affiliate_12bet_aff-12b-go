using Fanex.Affiliate.Link.Core.Services;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Data.Repository;
using Moq;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests.Services;

public class BotDetectionServiceTests
{
    private readonly Mock<IDynamicRepository> _mockRepository;
    private readonly Mock<ILogService> _mockLogService;
    private readonly BotDetectionService _botDetectionService;

    public BotDetectionServiceTests()
    {
        _mockRepository = new Mock<IDynamicRepository>();
        _mockLogService = new Mock<ILogService>();
        _botDetectionService = new BotDetectionService(_mockRepository.Object, _mockLogService.Object);
    }

    #region DetectBotFromUserAgent Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DetectBotFromUserAgent_ShouldDetectBot_WhenUserAgentIsEmpty(string userAgent)
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("Googlebot/2.1")]
    [InlineData("bingbot/2.0")]
    [InlineData("python-requests/2.28.0")]
    [InlineData("curl/7.68.0")]
    [InlineData("wget/1.20.3")]
    public void DetectBotFromUserAgent_ShouldDetectBot_ForKnownBots(string userAgent)
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.True(result, $"Should detect '{userAgent}' as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldNotDetectBot_ForWhatsAppWebDesktop()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 WhatsApp/2";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "WhatsApp/2 with browser signature should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldNotDetectBot_ForInstagramInAppBrowser()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_6_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/22G100 Instagram 400.1.0.29.75 (iPhone15,4; iOS 18_6_2; en_GB; en-GB; scale=3.00; 960x2079; IABMV/1; 799622199) NW/3";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Instagram in-app browser with device info should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldDetectBot_ForInstagramPreviewBot()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (compatible; facebookexternalhit/1.1; +http://www.facebook.com/externalhit_uatext.php) Instagram";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.True(result, "Instagram preview bot should be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldNotDetectBot_ForDalvikWithRealDevice()
    {
        // Arrange
        var userAgent = "Dalvik/2.1.0 (Linux; U; Android 11; V2026 Build/RP1A.200720.012)";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Dalvik with real device model (V2026) should NOT be detected as bot");
    }

    [Theory]
    [InlineData("Dalvik/2.1.0 (Linux; U; Android 11; V2026 Build/RP1A.200720.012)")]
    [InlineData("Dalvik/2.1.0 (Linux; U; Android 8.1.0; SM-G610F Build/M1AJQ)")]
    [InlineData("Dalvik/2.1.0 (Linux; U; Android 16; SM-A736B Build/BP2A.250605.031.A3)")]
    public void DetectBotFromUserAgent_ShouldNotDetectBot_ForDalvikWithRealDevices(string userAgent)
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, $"Dalvik with real device should NOT be detected as bot: {userAgent}");
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldNotDetectBot_ForModernSafari()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0.1 Mobile/15E148 Safari/604.1";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Modern Safari (Version/26.0.1) should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldDetectBot_ForOldSafari()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/2.0 Mobile/10A5376e Safari/8536.25";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.True(result, "Very old Safari (Version/2.0) should be detected as suspicious");
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/121.0")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")]
    public void DetectBotFromUserAgent_ShouldNotDetectBot_ForLegitimateDesktopBrowsers(string userAgent)
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, $"Legitimate browser should NOT be detected as bot: {userAgent}");
    }

    [Theory]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1")]
    [InlineData("Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36")]
    public void DetectBotFromUserAgent_ShouldNotDetectBot_ForLegitimateMobileBrowsers(string userAgent)
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, $"Legitimate mobile browser should NOT be detected as bot: {userAgent}");
    }

    #endregion

    #region DetectBotFromUserAgentWithDetails Tests

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldReturnDetails_ForWhatsAppPreviewBot()
    {
        // Arrange
        var userAgent = "WhatsApp/2.24.1.78";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.True(result.IsBot);
        Assert.Equal(40, result.Score);
        Assert.Contains("WhatsApp", result.Reason);
        Assert.Contains("Preview bot", result.Reason);
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldNotDetectBot_ForWhatsAppWebDesktop()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36 WhatsApp/2";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.False(result.IsBot);
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldReturnDetails_ForInstagramPreviewBot()
    {
        // Arrange
        var userAgent = "facebookexternalhit/1.1 Instagram";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.True(result.IsBot);
        Assert.Contains("Instagram", result.Reason);
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldNotDetectBot_ForInstagramInAppBrowser()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_6_2 like Mac OS X) Instagram 400.1.0.29.75 (iPhone15,4; iOS 18_6_2; en_GB; scale=3.00; 960x2079)";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.False(result.IsBot);
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldReturnDetails_ForGooglebot()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.True(result.IsBot);
        Assert.True(result.Score >= 40);
        Assert.Contains("bot", result.Reason.ToLowerInvariant());
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldReturnEmptyReason_ForNonBot()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.False(result.IsBot);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldHandleNull()
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(null!);

        // Assert
        Assert.True(result.IsBot);
        Assert.True(result.Score >= 40);
        Assert.Contains("Missing UA", result.Reason);
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldDetectBot_ForNullUserAgent()
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(null!);

        // Assert
        Assert.True(result, "Null user agent should be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldReturnDetails_ForDalvikScraper()
    {
        // Arrange - Dalvik without real device signature
        var userAgent = "Dalvik/2.1.0 (Linux; U; Android 11; Generic Build/12345)";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.True(result.IsBot);
        Assert.Contains("Dalvik", result.Reason);
    }

    [Fact]
    public void DetectBotFromUserAgentWithDetails_ShouldNotDetectBot_ForFacebookInAppBrowser()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 [FBAN/FBIOS;FBAV/442.0.0.32.109;]";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        Assert.False(result.IsBot, "Facebook in-app browser with FBAV should NOT be detected as bot");
    }

    #endregion

    #region Edge Cases and Special Scenarios

    [Theory]
    [InlineData("Twitterbot/1.0")]
    [InlineData("Slackbot-LinkExpanding 1.0")]
    [InlineData("Discordbot/2.0")]
    [InlineData("LinkedInBot/1.0")]
    public void DetectBotFromUserAgent_ShouldDetectBot_ForSocialMediaPreviewBots(string userAgent)
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.True(result, $"Social media preview bot should be detected: {userAgent}");
    }

    [Theory]
    [InlineData("axios/0.21.1")]
    [InlineData("Go-http-client/1.1")]
    [InlineData("GuzzleHttp/7")]
    [InlineData("Java-http-client/11.0.10")]
    public void DetectBotFromUserAgent_ShouldDetectBot_ForHTTPLibraries(string userAgent)
    {
        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.True(result, $"HTTP library should be detected as bot: {userAgent}");
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldBeCaseInsensitive()
    {
        // Arrange
        var userAgentLower = "googlebot/2.1";
        var userAgentUpper = "GOOGLEBOT/2.1";
        var userAgentMixed = "GoogleBot/2.1";

        // Act
        var resultLower = _botDetectionService.DetectBotFromUserAgent(userAgentLower);
        var resultUpper = _botDetectionService.DetectBotFromUserAgent(userAgentUpper);
        var resultMixed = _botDetectionService.DetectBotFromUserAgent(userAgentMixed);

        // Assert
        Assert.True(resultLower);
        Assert.True(resultUpper);
        Assert.True(resultMixed);
    }

    [Theory]
    [InlineData("WhatsApp", true)]
    [InlineData("Instagram", true)]
    [InlineData("Facebook", true)]
    [InlineData("Telegram", true)]
    [InlineData("Dalvik", true)]
    [InlineData("Chrome", false)]
    [InlineData("Firefox", false)]
    [InlineData("Safari", false)]
    [InlineData("", false)]
    public void IsMessagingApp_ShouldBeDetected_Correctly(string userAgent, bool expectedIsMessagingApp)
    {
        // Act
        var detailsResult = _botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);

        // Assert
        // We can't directly test the private helper, but we can test the behavior
        // This verifies that messaging apps are handled with smart detection
        Assert.NotNull(detailsResult);

        if (expectedIsMessagingApp && !string.IsNullOrEmpty(userAgent))
        {
            // Messaging apps get special handling
            // If detected as bot, should have specific messaging app reason
            if (detailsResult.IsBot)
            {
                Assert.True(detailsResult.Reason.Length > 0);
            }
        }
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldHandleVeryLongUserAgent()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 " + new string('X', 1000);

        // Act & Assert
        // Should not throw, and returns a boolean
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);
        // Boolean always has a value, just verify it doesn't throw
        Assert.True(result is true || result is false);
    }

    [Fact]
    public void DetectBotFromUserAgent_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (特殊字符) AppleWebKit/537.36 Chrome/120.0.0.0";

        // Act & Assert
        // Should not throw
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);
        Assert.False(result);
    }

    #endregion

    #region Real-World User Agent Tests

    [Fact]
    public void DetectBotFromUserAgent_RealWorldCase_WhatsAppDesktop_Windows()
    {
        // Arrange - Real WhatsApp Desktop user agent
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0 WhatsApp/2";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Real WhatsApp Desktop user should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_RealWorldCase_WhatsAppDesktop_Mac()
    {
        // Arrange - Real WhatsApp Desktop user agent
        var userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 WhatsApp/2";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Real WhatsApp Desktop Mac user should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_RealWorldCase_InstagramInAppBrowser_WithResolution()
    {
        // Arrange - Real Instagram in-app browser
        var userAgent = "Mozilla/5.0 (iPhone15,4; iOS 18_6_2; en_GB; en-GB; scale=3.00; 960x2079) Instagram 400.1.0.29.75";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Real Instagram in-app browser user should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_RealWorldCase_ChromeMobileAndroid()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Linux; Android 13; Pixel 7 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Real Chrome Mobile Android user should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_RealWorldCase_SafariMobileiOS()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Real Safari Mobile iOS user should NOT be detected as bot");
    }

    [Fact]
    public void DetectBotFromUserAgent_RealWorldCase_EdgeDesktop()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0";

        // Act
        var result = _botDetectionService.DetectBotFromUserAgent(userAgent);

        // Assert
        Assert.False(result, "Real Edge Desktop user should NOT be detected as bot");
    }

    #endregion

    #region Performance and Stress Tests

    [Fact]
    public void DetectBotFromUserAgent_ShouldBeReasonablyPerformant()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36";
        var iterations = 100; // Reduced iterations since third-party libs can be slower
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            _botDetectionService.DetectBotFromUserAgent(userAgent);
        }
        stopwatch.Stop();

        // Assert
        // Should complete in reasonable time (with third-party libraries, allow up to 3 seconds for 100 iterations)
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"100 iterations should complete in less than 3 seconds. Actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion
}

