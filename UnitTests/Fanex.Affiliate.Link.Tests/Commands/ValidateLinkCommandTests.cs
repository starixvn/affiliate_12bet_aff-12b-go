using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class ValidateLinkCommandTests
{
    [Fact]
    public void ValidateLinkCommand_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var command = new ValidateLinkCommand
        {
            AffCode = "TEST123",
            AffDomain = "test.com",
            PageCode = "home",
            LanguageCode = "en",
            RedirectPage = "https://example.com"
        };

        // Assert
        command.AffCode.Should().Be("TEST123");
        command.AffDomain.Should().Be("test.com");
        command.PageCode.Should().Be("home");
        command.LanguageCode.Should().Be("en");
        command.RedirectPage.Should().Be("https://example.com");
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var command = new ValidateLinkCommand();

        // Act
        var result = command.GetSettingKey();

        // Assert
        result.Should().Be("Aff_ClickStatistics_RedirectPage_Sel");
    }

    [Fact]
    public void IsValid_ShouldReturnTrue()
    {
        // Arrange
        var command = new ValidateLinkCommand();

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateLinkCommand_ShouldHandleNullValues()
    {
        // Arrange
        var command = new ValidateLinkCommand
        {
            AffCode = null,
            AffDomain = null,
            PageCode = null,
            LanguageCode = null,
            RedirectPage = null
        };

        // Assert
        command.AffCode.Should().BeNull();
        command.AffDomain.Should().BeNull();
        command.PageCode.Should().BeNull();
        command.LanguageCode.Should().BeNull();
        command.RedirectPage.Should().BeNull();
    }
}

