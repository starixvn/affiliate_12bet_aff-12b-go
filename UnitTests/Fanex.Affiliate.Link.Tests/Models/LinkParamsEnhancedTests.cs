using Fanex.Affiliate.Link.Models;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Models;

public class LinkParamsEnhancedTests
{
    [Fact]
    public void LinkParams_ShouldInitializeWithNullProperties()
    {
        // Arrange & Act
        var linkParams = new LinkParams();

        // Assert
        linkParams.Should().NotBeNull();
        linkParams.PageCode.Should().BeNull();
        linkParams.AffCode.Should().BeNull();
        linkParams.Lang.Should().BeNull();
        linkParams.Channel.Should().BeNull();
    }

    [Fact]
    public void LinkParams_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var linkParams = new LinkParams();
        var pageCode = "home";
        var affCode = "123";
        var lang = "en";
        var channel = "mobile";

        // Act
        linkParams.PageCode = pageCode;
        linkParams.AffCode = affCode;
        linkParams.Lang = lang;
        linkParams.Channel = channel;

        // Assert
        linkParams.PageCode.Should().Be(pageCode);
        linkParams.AffCode.Should().Be(affCode);
        linkParams.Lang.Should().Be(lang);
        linkParams.Channel.Should().Be(channel);
    }

    [Theory]
    [InlineData("", "", "", "")]
    [InlineData("home", "123", "en", "mobile")]
    [InlineData("sports", "456", "th", "desktop")]
    [InlineData("casino", "789", "vn", null)]
    public void LinkParams_ShouldAcceptVariousValues(string pageCode, string affCode, string lang, string channel)
    {
        // Arrange & Act
        var linkParams = new LinkParams
        {
            PageCode = pageCode,
            AffCode = affCode,
            Lang = lang,
            Channel = channel
        };

        // Assert
        linkParams.PageCode.Should().Be(pageCode);
        linkParams.AffCode.Should().Be(affCode);
        linkParams.Lang.Should().Be(lang);
        linkParams.Channel.Should().Be(channel);
    }

    [Fact]
    public void LinkParams_ShouldSupportObjectInitializer()
    {
        // Arrange & Act
        var linkParams = new LinkParams
        {
            PageCode = "signup",
            AffCode = "12345",
            Lang = "bd",
            Channel = "web"
        };

        // Assert
        linkParams.PageCode.Should().Be("signup");
        linkParams.AffCode.Should().Be("12345");
        linkParams.Lang.Should().Be("bd");
        linkParams.Channel.Should().Be("web");
    }

    [Fact]
    public void LinkParams_Properties_ShouldBeSettableIndependently()
    {
        // Arrange
        var linkParams = new LinkParams();

        // Act & Assert
        linkParams.PageCode = "test1";
        linkParams.PageCode.Should().Be("test1");
        linkParams.AffCode.Should().BeNull();

        linkParams.AffCode = "test2";
        linkParams.AffCode.Should().Be("test2");
        linkParams.PageCode.Should().Be("test1");

        linkParams.Lang = "test3";
        linkParams.Lang.Should().Be("test3");
        linkParams.PageCode.Should().Be("test1");
        linkParams.AffCode.Should().Be("test2");

        linkParams.Channel = "test4";
        linkParams.Channel.Should().Be("test4");
        linkParams.PageCode.Should().Be("test1");
        linkParams.AffCode.Should().Be("test2");
        linkParams.Lang.Should().Be("test3");
    }
}

