using Fanex.Affiliate.Link.Models;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Models;

public class LinkParamsTests
{
    [Fact]
    public void LinkParams_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var linkParams = new LinkParams
        {
            PageCode = "home",
            AffCode = "TEST123",
            Lang = "en",
            Channel = "web"
        };

        // Assert
        linkParams.PageCode.Should().Be("home");
        linkParams.AffCode.Should().Be("TEST123");
        linkParams.Lang.Should().Be("en");
        linkParams.Channel.Should().Be("web");
    }

    [Fact]
    public void LinkParams_ShouldHandleNullValues()
    {
        // Arrange
        var linkParams = new LinkParams
        {
            PageCode = null,
            AffCode = null,
            Lang = null,
            Channel = null
        };

        // Assert
        linkParams.PageCode.Should().BeNull();
        linkParams.AffCode.Should().BeNull();
        linkParams.Lang.Should().BeNull();
        linkParams.Channel.Should().BeNull();
    }

    [Fact]
    public void LinkParams_ShouldHandleEmptyStrings()
    {
        // Arrange
        var linkParams = new LinkParams
        {
            PageCode = "",
            AffCode = "",
            Lang = "",
            Channel = ""
        };

        // Assert
        linkParams.PageCode.Should().Be("");
        linkParams.AffCode.Should().Be("");
        linkParams.Lang.Should().Be("");
        linkParams.Channel.Should().Be("");
    }

    [Fact]
    public void LinkParams_ShouldBeMutable()
    {
        // Arrange
        var linkParams = new LinkParams();

        // Act
        linkParams.PageCode = "updated";
        linkParams.AffCode = "UPDATED123";
        linkParams.Lang = "th";
        linkParams.Channel = "mobile";

        // Assert
        linkParams.PageCode.Should().Be("updated");
        linkParams.AffCode.Should().Be("UPDATED123");
        linkParams.Lang.Should().Be("th");
        linkParams.Channel.Should().Be("mobile");
    }
}
