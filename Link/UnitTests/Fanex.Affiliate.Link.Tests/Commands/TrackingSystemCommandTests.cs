using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class TrackingSystemCommandTests
{
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
    public void IsValid_ShouldReturnTrue_WhenCalled()
    {
        // Arrange
        var command = new TrackingSystemCommand();

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var command = new TrackingSystemCommand
        {
            AffCode = "TEST123",
            AffDomain = "test.com",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            URLReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "91.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            RedirectPage = "https://example.com",
            ClickID = 12345,
            AffMediaLink = "https://affiliate.com/link"
        };

        // Assert
        command.AffCode.Should().Be("TEST123");
        command.AffDomain.Should().Be("test.com");
        command.PageCode.Should().Be("PAGE001");
        command.LanguageCode.Should().Be("en");
        command.ChannelCode.Should().Be("CHANNEL001");
        command.IP.Should().Be("192.168.1.1");
        command.Browser.Should().Be("Chrome");
        command.URLReferrer.Should().Be("https://google.com");
        command.DomainReferrer.Should().Be("google.com");
        command.OS.Should().Be("Windows");
        command.Device.Should().Be("Desktop");
        command.BrowserVersion.Should().Be("91.0");
        command.BrowserResolution.Should().Be("1920x1080");
        command.BrowserLanguage.Should().Be("en-US");
        command.UserAgent.Should().Be("Mozilla/5.0...");
        command.Country.Should().Be("US");
        command.DeviceType.Should().Be(1);
        command.RedirectPage.Should().Be("https://example.com");
        command.ClickID.Should().Be(12345);
        command.AffMediaLink.Should().Be("https://affiliate.com/link");
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenPropertiesAreSet()
    {
        // Arrange
        var command = new TrackingSystemCommand
        {
            AffCode = "TEST123",
            AffDomain = "test.com",
            PageCode = "PAGE001"
        };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeTrue();
    }
}
