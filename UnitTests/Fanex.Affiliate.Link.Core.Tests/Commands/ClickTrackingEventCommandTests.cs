using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests.Commands;

public class ClickTrackingEventCommandTests
{
    [Fact]
    public void ClickTrackingEventCommand_ShouldCreateInstance_WithAllRequiredProperties()
    {
        // Arrange & Act
        var command = new ClickTrackingEventCommand
        {
            AffCode = "AFF001",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            URLReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            AffMediaLink = "https://affiliate.com/link",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            RedirectURL = "https://target.com",
            ErrMsg = "Test error message"
        };

        // Assert
        command.Should().NotBeNull();
        command.AffCode.Should().Be("AFF001");
        command.PageCode.Should().Be("PAGE001");
        command.LanguageCode.Should().Be("en");
        command.ChannelCode.Should().Be("CHANNEL001");
        command.IP.Should().Be("192.168.1.1");
        command.Browser.Should().Be("Chrome");
        command.URLReferrer.Should().Be("https://google.com");
        command.DomainReferrer.Should().Be("google.com");
        command.OS.Should().Be("Windows");
        command.Device.Should().Be("Desktop");
        command.BrowserVersion.Should().Be("120.0.0.0");
        command.BrowserResolution.Should().Be("1920x1080");
        command.BrowserLanguage.Should().Be("en-US");
        command.UserAgent.Should().Be("Mozilla/5.0...");
        command.Country.Should().Be("US");
        command.DeviceType.Should().Be(1);
        command.AffMediaLink.Should().Be("https://affiliate.com/link");
        command.ClickId.Should().Be(12345);
        command.UpdatedTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        command.RedirectURL.Should().Be("https://target.com");
        command.ErrMsg.Should().Be("Test error message");
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectValue()
    {
        // Arrange
        var command = new ClickTrackingEventCommand
        {
            AffCode = "AFF001",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            URLReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            AffMediaLink = "https://affiliate.com/link",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            RedirectURL = "https://target.com",
            ErrMsg = "Test error message"
        };

        // Act
        var settingKey = command.GetSettingKey();

        // Assert
        settingKey.Should().Be("Aff_ClickStatisticsIns");
    }

    [Fact]
    public void IsValid_ShouldReturnTrue()
    {
        // Arrange
        var command = new ClickTrackingEventCommand
        {
            AffCode = "AFF001",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            URLReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            AffMediaLink = "https://affiliate.com/link",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            RedirectURL = "https://target.com",
            ErrMsg = "Test error message"
        };

        // Act
        var isValid = command.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ClickTrackingEventCommand_ShouldInheritFromBaseNonQueryCommand()
    {
        // Arrange & Act
        var command = new ClickTrackingEventCommand
        {
            AffCode = "AFF001",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            URLReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            AffMediaLink = "https://affiliate.com/link",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            RedirectURL = "https://target.com",
            ErrMsg = "Test error message"
        };

        // Assert
        command.Should().BeAssignableTo<Fanex.Affiliate.Link.Core.Commands.BaseNonQueryCommand>();
    }

    [Fact]
    public void ClickTrackingEventCommand_ShouldAllowEmptyStrings()
    {
        // Arrange & Act
        var command = new ClickTrackingEventCommand
        {
            AffCode = "",
            PageCode = "",
            LanguageCode = "",
            ChannelCode = "",
            IP = "",
            Browser = "",
            URLReferrer = "",
            DomainReferrer = "",
            OS = "",
            Device = "",
            BrowserVersion = "",
            BrowserResolution = "",
            BrowserLanguage = "",
            UserAgent = "",
            Country = "",
            DeviceType = 0,
            AffMediaLink = "",
            ClickId = 0,
            UpdatedTime = DateTime.MinValue,
            RedirectURL = "",
            ErrMsg = ""
        };

        // Assert
        command.Should().NotBeNull();
        command.AffCode.Should().Be("");
        command.ErrMsg.Should().Be("");
    }
}

