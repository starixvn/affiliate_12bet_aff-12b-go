using Fanex.Affiliate.Link.Models;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Models;

public class ValidateResultEnhancedTests
{
    [Fact]
    public void ValidateResult_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var validateResult = new ValidateResult();

        // Assert
        validateResult.Should().NotBeNull();
        validateResult.IsValid.Should().BeFalse();
        validateResult.ErrorMessage.Should().BeNull();
        validateResult.RedirectLink.Should().BeNull();
        validateResult.IsAffMarketing.Should().BeFalse();
    }

    [Fact]
    public void ValidateResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var validateResult = new ValidateResult();
        var errorMessage = "Test error message";
        var redirectView = "Error";

        // Act
        validateResult.IsValid = true;
        validateResult.ErrorMessage = errorMessage;
        validateResult.RedirectLink = redirectView;
        validateResult.IsAffMarketing = true;

        // Assert
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().Be(errorMessage);
        validateResult.RedirectLink.Should().Be(redirectView);
        validateResult.IsAffMarketing.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, "Success", "Home", false)]
    [InlineData(false, "Validation failed", "Error", true)]
    [InlineData(true, null, null, false)]
    [InlineData(false, "", "", true)]
    public void ValidateResult_ShouldAcceptVariousValues(bool isValid, string errorMessage, string redirectView, bool isAffMarketing)
    {
        // Arrange & Act
        var validateResult = new ValidateResult
        {
            IsValid = isValid,
            ErrorMessage = errorMessage,
            RedirectLink = redirectView,
            IsAffMarketing = isAffMarketing
        };

        // Assert
        validateResult.IsValid.Should().Be(isValid);
        validateResult.ErrorMessage.Should().Be(errorMessage);
        validateResult.RedirectLink.Should().Be(redirectView);
        validateResult.IsAffMarketing.Should().Be(isAffMarketing);
    }

    [Fact]
    public void ValidateResult_ShouldSupportObjectInitializer()
    {
        // Arrange & Act
        var validateResult = new ValidateResult
        {
            IsValid = true,
            ErrorMessage = "All good",
            RedirectLink = "Link",
            IsAffMarketing = false
        };

        // Assert
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().Be("All good");
        validateResult.RedirectLink.Should().Be("Link");
        validateResult.IsAffMarketing.Should().BeFalse();
    }

    [Fact]
    public void ValidateResult_Properties_ShouldBeSettableIndependently()
    {
        // Arrange
        var validateResult = new ValidateResult();

        // Act & Assert
        validateResult.IsValid = true;
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().BeNull();
        
        validateResult.ErrorMessage = "test error";
        validateResult.ErrorMessage.Should().Be("test error");
        validateResult.IsValid.Should().BeTrue();
        
        validateResult.RedirectLink = "test view";
        validateResult.RedirectLink.Should().Be("test view");
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().Be("test error");
        
        validateResult.IsAffMarketing = true;
        validateResult.IsAffMarketing.Should().BeTrue();
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().Be("test error");
        validateResult.RedirectLink.Should().Be("test view");
    }

    [Fact]
    public void ValidateResult_ShouldHandleSuccessScenario()
    {
        // Arrange & Act
        var validateResult = new ValidateResult
        {
            IsValid = true,
            RedirectLink = "Link"
        };

        // Assert
        validateResult.IsValid.Should().BeTrue();
        validateResult.ErrorMessage.Should().BeNull();
        validateResult.RedirectLink.Should().Be("Link");
        validateResult.IsAffMarketing.Should().BeFalse();
    }

    [Fact]
    public void ValidateResult_ShouldHandleErrorScenario()
    {
        // Arrange & Act
        var validateResult = new ValidateResult
        {
            IsValid = false,
            ErrorMessage = "Invalid affiliate code",
            RedirectLink = "Error"
        };

        // Assert
        validateResult.IsValid.Should().BeFalse();
        validateResult.ErrorMessage.Should().Be("Invalid affiliate code");
        validateResult.RedirectLink.Should().Be("Error");
        validateResult.IsAffMarketing.Should().BeFalse();
    }

    [Fact]
    public void ValidateResult_ShouldHandleMarketingScenario()
    {
        // Arrange & Act
        var validateResult = new ValidateResult
        {
            IsValid = true,
            IsAffMarketing = true,
            RedirectLink = "Marketing"
        };

        // Assert
        validateResult.IsValid.Should().BeTrue();
        validateResult.IsAffMarketing.Should().BeTrue();
        validateResult.RedirectLink.Should().Be("Marketing");
        validateResult.ErrorMessage.Should().BeNull();
    }
}

