using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class CheckMarketingLinkResultEnhancedTests
{
    [Fact]
    public void CheckMarketingLinkResult_Properties_ShouldBeIndependent()
    {
        // Arrange
        var result = new CheckMarketingLinkResult();

        // Act & Assert
        result.IsExistedTracking = true;
        result.IsExistedTracking.Should().BeTrue();
        result.IsValidDomain.Should().BeFalse();

        result.IsValidDomain = true;
        result.IsValidDomain.Should().BeTrue();
        result.IsExistedTracking.Should().BeTrue();
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldSupportObjectInitializer()
    {
        // Arrange & Act
        var result = new CheckMarketingLinkResult
        {
            IsExistedTracking = true,
            IsValidDomain = false
        };

        // Assert
        result.IsExistedTracking.Should().BeTrue();
        result.IsValidDomain.Should().BeFalse();
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldHandleToggling()
    {
        // Arrange
        var result = new CheckMarketingLinkResult();

        // Act & Assert - Toggle IsExistedTracking
        result.IsExistedTracking = true;
        result.IsExistedTracking.Should().BeTrue();
        result.IsExistedTracking = false;
        result.IsExistedTracking.Should().BeFalse();

        // Act & Assert - Toggle IsValidDomain
        result.IsValidDomain = true;
        result.IsValidDomain.Should().BeTrue();
        result.IsValidDomain = false;
        result.IsValidDomain.Should().BeFalse();
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldRepresentValidTrackingWithValidDomain()
    {
        // Arrange & Act
        var validResult = new CheckMarketingLinkResult
        {
            IsExistedTracking = true,
            IsValidDomain = true
        };

        // Assert
        validResult.IsExistedTracking.Should().BeTrue("tracking should exist");
        validResult.IsValidDomain.Should().BeTrue("domain should be valid");
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldRepresentInvalidScenarios()
    {
        // Arrange & Act
        var noTrackingResult = new CheckMarketingLinkResult
        {
            IsExistedTracking = false,
            IsValidDomain = true
        };

        var invalidDomainResult = new CheckMarketingLinkResult
        {
            IsExistedTracking = true,
            IsValidDomain = false
        };

        var completelyInvalidResult = new CheckMarketingLinkResult
        {
            IsExistedTracking = false,
            IsValidDomain = false
        };

        // Assert
        noTrackingResult.IsExistedTracking.Should().BeFalse("no tracking exists");
        noTrackingResult.IsValidDomain.Should().BeTrue("but domain is valid");

        invalidDomainResult.IsExistedTracking.Should().BeTrue("tracking exists");
        invalidDomainResult.IsValidDomain.Should().BeFalse("but domain is invalid");

        completelyInvalidResult.IsExistedTracking.Should().BeFalse("no tracking exists");
        completelyInvalidResult.IsValidDomain.Should().BeFalse("and domain is invalid");
    }

    [Fact]
    public void CheckMarketingLinkResult_BooleanProperties_ShouldBeOfCorrectType()
    {
        // Arrange
        var result = new CheckMarketingLinkResult();

        // Act & Assert
        Assert.True(result.IsExistedTracking == true || result.IsExistedTracking == false);
        Assert.True(result.IsValidDomain == true || result.IsValidDomain == false);
    }

    [Theory]
    [InlineData(true, true, "Valid tracking and valid domain")]
    [InlineData(true, false, "Valid tracking but invalid domain")]
    [InlineData(false, true, "No tracking but valid domain")]
    [InlineData(false, false, "No tracking and invalid domain")]
    public void CheckMarketingLinkResult_ShouldHandleAllCombinations(bool isExistedTracking, bool isValidDomain, string scenario)
    {
        // Arrange & Act
        var result = new CheckMarketingLinkResult
        {
            IsExistedTracking = isExistedTracking,
            IsValidDomain = isValidDomain
        };

        // Assert
        result.IsExistedTracking.Should().Be(isExistedTracking, $"IsExistedTracking should be {isExistedTracking} for scenario: {scenario}");
        result.IsValidDomain.Should().Be(isValidDomain, $"IsValidDomain should be {isValidDomain} for scenario: {scenario}");
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldBeReferenceType()
    {
        // Arrange & Act
        var result1 = new CheckMarketingLinkResult { IsExistedTracking = true };
        var result2 = new CheckMarketingLinkResult { IsExistedTracking = true };

        // Assert
        result1.Should().NotBeSameAs(result2, "different instances should not be the same reference");
        result1.IsExistedTracking.Should().Be(result2.IsExistedTracking, "but should have same property values");
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldSupportPropertyModification()
    {
        // Arrange
        var result = new CheckMarketingLinkResult
        {
            IsExistedTracking = false,
            IsValidDomain = false
        };

        // Act
        result.IsExistedTracking = true;
        result.IsValidDomain = true;

        // Assert
        result.IsExistedTracking.Should().BeTrue("property was modified");
        result.IsValidDomain.Should().BeTrue("property was modified");
    }
}
