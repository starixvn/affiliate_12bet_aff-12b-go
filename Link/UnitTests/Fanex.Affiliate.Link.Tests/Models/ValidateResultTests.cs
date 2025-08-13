using Fanex.Affiliate.Link.Models;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Models;

public class ValidateResultTests
{
    [Fact]
    public void ValidateResult_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var validateResult = new ValidateResult
        {
            IsValid = true,
            ErrorMessage = "Success",
            RedirectLink = "https://example.com",
            IsAffMarketing = true
        };

        // Assert
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().Be("Success");
        validateResult.RedirectLink.Should().Be("https://example.com");
        validateResult.IsAffMarketing.Should().BeTrue();
    }

    [Fact]
    public void ValidateResult_ShouldHandleInvalidResult()
    {
        // Arrange
        var validateResult = new ValidateResult
        {
            IsValid = false,
            ErrorMessage = "Invalid link",
            RedirectLink = null,
            IsAffMarketing = false
        };

        // Assert
        validateResult.IsValid.Should().BeFalse();
        validateResult.ErrorMessage.Should().Be("Invalid link");
        validateResult.RedirectLink.Should().BeNull();
        validateResult.IsAffMarketing.Should().BeFalse();
    }

    [Fact]
    public void ValidateResult_ShouldHandleNullValues()
    {
        // Arrange
        var validateResult = new ValidateResult
        {
            IsValid = false,
            ErrorMessage = null,
            RedirectLink = null,
            IsAffMarketing = false
        };

        // Assert
        validateResult.IsValid.Should().BeFalse();
        validateResult.ErrorMessage.Should().BeNull();
        validateResult.RedirectLink.Should().BeNull();
        validateResult.IsAffMarketing.Should().BeFalse();
    }

    [Fact]
    public void ValidateResult_ShouldBeMutable()
    {
        // Arrange
        var validateResult = new ValidateResult();

        // Act
        validateResult.IsValid = true;
        validateResult.ErrorMessage = "Updated message";
        validateResult.RedirectLink = "https://updated.com";
        validateResult.IsAffMarketing = true;

        // Assert
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().Be("Updated message");
        validateResult.RedirectLink.Should().Be("https://updated.com");
        validateResult.IsAffMarketing.Should().BeTrue();
    }

    [Fact]
    public void ValidateResult_ShouldHandleDefaultValues()
    {
        // Arrange
        var validateResult = new ValidateResult();

        // Assert
        validateResult.IsValid.Should().BeFalse();
        validateResult.ErrorMessage.Should().BeNull();
        validateResult.RedirectLink.Should().BeNull();
        validateResult.IsAffMarketing.Should().BeFalse();
    }
}

