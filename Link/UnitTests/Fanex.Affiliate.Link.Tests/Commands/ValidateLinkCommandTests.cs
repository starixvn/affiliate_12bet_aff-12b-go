using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class ValidateLinkCommandTests
{
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
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var command = new ValidateLinkCommand
        {
            AffCode = "TEST001",
            AffDomain = "test.com",
            PageCode = "PAGE001",
            LanguageCode = "en",
            RedirectPage = "https://example.com",
            ErrCode = 0,
            ErrMsg = "Success"
        };

        // Assert
        command.AffCode.Should().Be("TEST001");
        command.AffDomain.Should().Be("test.com");
        command.PageCode.Should().Be("PAGE001");
        command.LanguageCode.Should().Be("en");
        command.RedirectPage.Should().Be("https://example.com");
        command.ErrCode.Should().Be(0);
        command.ErrMsg.Should().Be("Success");
    }

    [Fact]
    public void Properties_ShouldBeGettable()
    {
        // Arrange
        var command = new ValidateLinkCommand();

        // Act
        command.AffCode = "TEST001";
        command.AffDomain = "test.com";
        command.PageCode = "PAGE001";
        command.LanguageCode = "en";
        command.RedirectPage = "https://example.com";
        command.ErrCode = 1;
        command.ErrMsg = "Error";

        // Assert
        command.AffCode.Should().Be("TEST001");
        command.AffDomain.Should().Be("test.com");
        command.PageCode.Should().Be("PAGE001");
        command.LanguageCode.Should().Be("en");
        command.RedirectPage.Should().Be("https://example.com");
        command.ErrCode.Should().Be(1);
        command.ErrMsg.Should().Be("Error");
    }

    [Fact]
    public void Properties_ShouldHandleNullValues()
    {
        // Arrange
        var command = new ValidateLinkCommand();

        // Act
        command.AffCode = null!;
        command.AffDomain = null!;
        command.PageCode = null!;
        command.LanguageCode = null!;
        command.RedirectPage = null!;
        command.ErrMsg = null!;

        // Assert
        command.AffCode.Should().BeNull();
        command.AffDomain.Should().BeNull();
        command.PageCode.Should().BeNull();
        command.LanguageCode.Should().BeNull();
        command.RedirectPage.Should().BeNull();
        command.ErrMsg.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldHandleEmptyStrings()
    {
        // Arrange
        var command = new ValidateLinkCommand();

        // Act
        command.AffCode = "";
        command.AffDomain = "";
        command.PageCode = "";
        command.LanguageCode = "";
        command.RedirectPage = "";
        command.ErrMsg = "";

        // Assert
        command.AffCode.Should().Be("");
        command.AffDomain.Should().Be("");
        command.PageCode.Should().Be("");
        command.LanguageCode.Should().Be("");
        command.RedirectPage.Should().Be("");
        command.ErrMsg.Should().Be("");
    }
}

