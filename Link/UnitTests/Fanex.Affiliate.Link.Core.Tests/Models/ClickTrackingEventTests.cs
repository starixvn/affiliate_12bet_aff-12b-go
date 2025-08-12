using Fanex.Affiliate.Link.Core.Models;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests.Models;

public class ClickTrackingEventTests
{
    [Fact]
    public void ClickTrackingEvent_ShouldCreateInstance_WithAllRequiredProperties()
    {
        // Arrange & Act
        var clickEvent = new ClickTrackingEvent
        {
            AffCode = "AFF001",
            AffDomain = "example.com",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            UrlReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            RedirectLink = "https://target.com",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            AffMediaLink = "https://affiliate.com/link"
        };

        // Assert
        clickEvent.Should().NotBeNull();
        clickEvent.AffCode.Should().Be("AFF001");
        clickEvent.AffDomain.Should().Be("example.com");
        clickEvent.PageCode.Should().Be("PAGE001");
        clickEvent.LanguageCode.Should().Be("en");
        clickEvent.ChannelCode.Should().Be("CHANNEL001");
        clickEvent.IP.Should().Be("192.168.1.1");
        clickEvent.Browser.Should().Be("Chrome");
        clickEvent.UrlReferrer.Should().Be("https://google.com");
        clickEvent.DomainReferrer.Should().Be("google.com");
        clickEvent.OS.Should().Be("Windows");
        clickEvent.Device.Should().Be("Desktop");
        clickEvent.BrowserVersion.Should().Be("120.0.0.0");
        clickEvent.BrowserResolution.Should().Be("1920x1080");
        clickEvent.BrowserLanguage.Should().Be("en-US");
        clickEvent.UserAgent.Should().Be("Mozilla/5.0...");
        clickEvent.Country.Should().Be("US");
        clickEvent.DeviceType.Should().Be(1);
        clickEvent.RedirectLink.Should().Be("https://target.com");
        clickEvent.ClickId.Should().Be(12345);
        clickEvent.UpdatedTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        clickEvent.AffMediaLink.Should().Be("https://affiliate.com/link");
    }

    [Fact]
    public void ClickTrackingEvent_ShouldAllowDeviceTypeZero()
    {
        // Arrange & Act
        var clickEvent = new ClickTrackingEvent
        {
            AffCode = "AFF001",
            AffDomain = "example.com",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            UrlReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 0,
            RedirectLink = "https://target.com",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            AffMediaLink = "https://affiliate.com/link"
        };

        // Assert
        clickEvent.DeviceType.Should().Be(0);
    }

    [Fact]
    public void ClickTrackingEvent_ShouldAllowDeviceType255()
    {
        // Arrange & Act
        var clickEvent = new ClickTrackingEvent
        {
            AffCode = "AFF001",
            AffDomain = "example.com",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            UrlReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 255,
            RedirectLink = "https://target.com",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            AffMediaLink = "https://affiliate.com/link"
        };

        // Assert
        clickEvent.DeviceType.Should().Be(255);
    }

    [Fact]
    public void ClickTrackingEvent_ShouldAllowNegativeClickId()
    {
        // Arrange & Act
        var clickEvent = new ClickTrackingEvent
        {
            AffCode = "AFF001",
            AffDomain = "example.com",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            UrlReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            RedirectLink = "https://target.com",
            ClickId = -1,
            UpdatedTime = DateTime.UtcNow,
            AffMediaLink = "https://affiliate.com/link"
        };

        // Assert
        clickEvent.ClickId.Should().Be(-1);
    }
}
