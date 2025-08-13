using System;
using Fanex.Affiliate.Link.Core.Models;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class ClickTrackingEventModelTests
{
    [Fact]
    public void ClickTrackingEvent_ShouldCreateInstance_WithRequiredProperties()
    {
        // Arrange & Act
        var clickEvent = new ClickTrackingEvent
        {
            ClickId = 123,
            AffCode = "TEST",
            AffDomain = "test.com",
            PageCode = "PAGE1",
            LanguageCode = "EN",
            ChannelCode = "WEB",
            IP = "127.0.0.1",
            Browser = "Chrome",
            UrlReferrer = "http://test.com",
            DomainReferrer = "test.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "100.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0",
            Country = "US",
            RedirectLink = "http://redirect.com",
            AffMediaLink = "http://media.com",
            DeviceType = 1,
            UpdatedTime = DateTime.UtcNow
        };

        // Assert
        clickEvent.Should().NotBeNull();
        clickEvent.ClickId.Should().Be(123);
        clickEvent.AffCode.Should().Be("TEST");
        clickEvent.AffDomain.Should().Be("test.com");
        clickEvent.PageCode.Should().Be("PAGE1");
        clickEvent.LanguageCode.Should().Be("EN");
        clickEvent.ChannelCode.Should().Be("WEB");
        clickEvent.IP.Should().Be("127.0.0.1");
        clickEvent.Browser.Should().Be("Chrome");
        clickEvent.UrlReferrer.Should().Be("http://test.com");
        clickEvent.DomainReferrer.Should().Be("test.com");
        clickEvent.OS.Should().Be("Windows");
        clickEvent.Device.Should().Be("Desktop");
        clickEvent.BrowserVersion.Should().Be("100.0");
        clickEvent.BrowserResolution.Should().Be("1920x1080");
        clickEvent.BrowserLanguage.Should().Be("en-US");
        clickEvent.UserAgent.Should().Be("Mozilla/5.0");
        clickEvent.Country.Should().Be("US");
        clickEvent.RedirectLink.Should().Be("http://redirect.com");
        clickEvent.AffMediaLink.Should().Be("http://media.com");
        clickEvent.DeviceType.Should().Be(1);
        clickEvent.UpdatedTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ClickTrackingEvent_ShouldAllowPropertyModification()
    {
        // Arrange
        var clickEvent = CreateValidClickTrackingEvent();

        // Act
        clickEvent.AffCode = "MODIFIED";
        clickEvent.ClickId = 999;
        clickEvent.DeviceType = 2;

        // Assert
        clickEvent.AffCode.Should().Be("MODIFIED");
        clickEvent.ClickId.Should().Be(999);
        clickEvent.DeviceType.Should().Be(2);
    }

    [Fact]
    public void ClickTrackingEvent_ToString_ShouldReturnStringRepresentation()
    {
        // Arrange
        var clickEvent = CreateValidClickTrackingEvent();

        // Act
        var result = clickEvent.ToString();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    private static ClickTrackingEvent CreateValidClickTrackingEvent()
    {
        return new ClickTrackingEvent
        {
            ClickId = 123,
            AffCode = "TEST",
            AffDomain = "test.com",
            PageCode = "PAGE1",
            LanguageCode = "EN",
            ChannelCode = "WEB",
            IP = "127.0.0.1",
            Browser = "Chrome",
            UrlReferrer = "http://test.com",
            DomainReferrer = "test.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "100.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0",
            Country = "US",
            RedirectLink = "http://redirect.com",
            AffMediaLink = "http://media.com",
            DeviceType = 1,
            UpdatedTime = DateTime.UtcNow
        };
    }
}
