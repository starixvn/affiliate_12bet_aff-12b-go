using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class ToggleMarketingLinkCommandTests
{
    [Fact]
    public void ToggleMarketingLinkCommand_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var command = new ToggleMarketingLinkCommand
        {
            AffId = 123,
            ChannelCode = "web",
            LanguageCode = "en",
            AffMediaLink = "https://example.com"
        };

        // Assert
        command.AffId.Should().Be(123);
        command.ChannelCode.Should().Be("web");
        command.LanguageCode.Should().Be("en");
        command.AffMediaLink.Should().Be("https://example.com");
        command.IsOn.Should().BeTrue();
    }

    [Fact]
    public void ToggleMarketingLinkCommand_ShouldHandleNullValues()
    {
        // Arrange
        var command = new ToggleMarketingLinkCommand
        {
            AffId = 0,
            ChannelCode = null,
            LanguageCode = null,
            AffMediaLink = null
        };

        // Assert
        command.AffId.Should().Be(0);
        command.ChannelCode.Should().BeNull();
        command.LanguageCode.Should().BeNull();
        command.AffMediaLink.Should().BeNull();
        command.IsOn.Should().BeTrue();
    }

    [Fact]
    public void ToggleMarketingLinkCommand_ShouldHandleDefaultValues()
    {
        // Arrange
        var command = new ToggleMarketingLinkCommand();

        // Assert
        command.AffId.Should().Be(0);
        command.ChannelCode.Should().BeNull();
        command.LanguageCode.Should().BeNull();
        command.AffMediaLink.Should().BeNull();
        command.IsOn.Should().BeTrue();
    }
}
