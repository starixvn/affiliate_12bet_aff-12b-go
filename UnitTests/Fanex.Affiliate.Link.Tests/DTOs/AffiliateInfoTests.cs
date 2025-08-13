using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class AffiliateInfoTests
{
    [Fact]
    public void AffiliateInfo_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var affiliateInfo = new AffiliateInfo
        {
            AffId = 123,
            AffCode = "TEST123"
        };

        // Assert
        affiliateInfo.AffId.Should().Be(123);
        affiliateInfo.AffCode.Should().Be("TEST123");
    }

    [Fact]
    public void AffiliateInfo_ShouldHandleDefaultValues()
    {
        // Arrange
        var affiliateInfo = new AffiliateInfo();

        // Assert
        affiliateInfo.AffId.Should().Be(0);
        affiliateInfo.AffCode.Should().BeNull();
    }

    [Fact]
    public void AffiliateInfo_ShouldBeMutable()
    {
        // Arrange
        var affiliateInfo = new AffiliateInfo();

        // Act
        affiliateInfo.AffId = 456;
        affiliateInfo.AffCode = "UPDATED123";

        // Assert
        affiliateInfo.AffId.Should().Be(456);
        affiliateInfo.AffCode.Should().Be("UPDATED123");
    }
}
