using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class CheckMarketingLinkResultTests
{
    [Fact]
    public void CheckMarketingLinkResult_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var result = new CheckMarketingLinkResult
        {
            IsExistedTracking = true,
            IsValidDomain = true
        };

        // Assert
        result.IsExistedTracking.Should().BeTrue();
        result.IsValidDomain.Should().BeTrue();
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldHandleDefaultValues()
    {
        // Arrange
        var result = new CheckMarketingLinkResult();

        // Assert
        result.IsExistedTracking.Should().BeFalse();
        result.IsValidDomain.Should().BeFalse();
    }

    [Fact]
    public void CheckMarketingLinkResult_ShouldBeMutable()
    {
        // Arrange
        var result = new CheckMarketingLinkResult();

        // Act
        result.IsExistedTracking = true;
        result.IsValidDomain = false;

        // Assert
        result.IsExistedTracking.Should().BeTrue();
        result.IsValidDomain.Should().BeFalse();
    }
}
