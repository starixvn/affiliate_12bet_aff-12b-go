using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class TrackingSystemCommandTests
{
    [Fact]
    public void TrackingSystemCommand_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var command = new TrackingSystemCommand
        {
            AffCode = "TEST123",
            AffDomain = "test.com",
            PageCode = "home",
            LanguageCode = "en",
            ChannelCode = "web",
            IP = "192.168.1.1",
            Browser = "Chrome",
            URLReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "100.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            RedirectPage = "https://example.com",
            ClickID = 12345,
            AffMediaLink = "https://affiliate.com"
        };

        // Assert
        command.AffCode.Should().Be("TEST123");
        command.AffDomain.Should().Be("test.com");
        command.PageCode.Should().Be("home");
        command.LanguageCode.Should().Be("en");
        command.ChannelCode.Should().Be("web");
        command.IP.Should().Be("192.168.1.1");
        command.Browser.Should().Be("Chrome");
        command.URLReferrer.Should().Be("https://google.com");
        command.DomainReferrer.Should().Be("google.com");
        command.OS.Should().Be("Windows");
        command.Device.Should().Be("Desktop");
        command.BrowserVersion.Should().Be("100.0");
        command.BrowserResolution.Should().Be("1920x1080");
        command.BrowserLanguage.Should().Be("en-US");
        command.UserAgent.Should().Be("Mozilla/5.0...");
        command.Country.Should().Be("US");
        command.DeviceType.Should().Be(1);
        command.RedirectPage.Should().Be("https://example.com");
        command.ClickID.Should().Be(12345);
        command.AffMediaLink.Should().Be("https://affiliate.com");
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var command = new TrackingSystemCommand();

        // Act
        var result = command.GetSettingKey();

        // Assert
        result.Should().Be("Aff_ClickStatistics_Ins");
    }

    [Fact]
    public void IsValid_ShouldReturnTrue()
    {
        // Arrange
        var command = new TrackingSystemCommand();

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TrackingSystemCommand_ShouldHandleNullValues()
    {
        // Arrange
        var command = new TrackingSystemCommand
        {
            AffCode = null,
            AffDomain = null,
            PageCode = null,
            LanguageCode = null,
            ChannelCode = null,
            IP = null,
            Browser = null,
            URLReferrer = null,
            DomainReferrer = null,
            OS = null,
            Device = null,
            BrowserVersion = null,
            BrowserResolution = null,
            BrowserLanguage = null,
            UserAgent = null,
            Country = null,
            RedirectPage = null,
            AffMediaLink = null
        };

        // Assert
        command.AffCode.Should().BeNull();
        command.AffDomain.Should().BeNull();
        command.PageCode.Should().BeNull();
        command.LanguageCode.Should().BeNull();
        command.ChannelCode.Should().BeNull();
        command.IP.Should().BeNull();
        command.Browser.Should().BeNull();
        command.URLReferrer.Should().BeNull();
        command.DomainReferrer.Should().BeNull();
        command.OS.Should().BeNull();
        command.Device.Should().BeNull();
        command.BrowserVersion.Should().BeNull();
        command.BrowserResolution.Should().BeNull();
        command.BrowserLanguage.Should().BeNull();
        command.UserAgent.Should().BeNull();
        command.Country.Should().BeNull();
        command.RedirectPage.Should().BeNull();
        command.AffMediaLink.Should().BeNull();
    }

    [Fact]
    public void TrackingSystemCommand_ShouldHandleDefaultValues()
    {
        // Arrange
        var command = new TrackingSystemCommand();

        // Assert
        command.ClickID.Should().Be(0);
        command.DeviceType.Should().Be(0);
    }
}
