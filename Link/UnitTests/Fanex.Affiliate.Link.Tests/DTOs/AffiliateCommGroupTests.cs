using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class AffiliateCommGroupTests
{
    [Fact]
    public void AffiliateCommGroup_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var affiliateCommGroup = new AffiliateCommGroup
        {
            AffCode = "TEST123",
            GroupName = "Test Group",
            TargetCountryCode = "US"
        };

        // Assert
        affiliateCommGroup.AffCode.Should().Be("TEST123");
        affiliateCommGroup.GroupName.Should().Be("Test Group");
        affiliateCommGroup.TargetCountryCode.Should().Be("US");
    }

    [Fact]
    public void AffiliateCommGroup_ShouldHandleDefaultValues()
    {
        // Arrange
        var affiliateCommGroup = new AffiliateCommGroup();

        // Assert
        affiliateCommGroup.AffCode.Should().BeNull();
        affiliateCommGroup.GroupName.Should().BeNull();
        affiliateCommGroup.TargetCountryCode.Should().BeNull();
    }

    [Fact]
    public void AffiliateCommGroup_ShouldBeMutable()
    {
        // Arrange
        var affiliateCommGroup = new AffiliateCommGroup();

        // Act
        affiliateCommGroup.AffCode = "UPDATED123";
        affiliateCommGroup.GroupName = "Updated Group";
        affiliateCommGroup.TargetCountryCode = "UK";

        // Assert
        affiliateCommGroup.AffCode.Should().Be("UPDATED123");
        affiliateCommGroup.GroupName.Should().Be("Updated Group");
        affiliateCommGroup.TargetCountryCode.Should().Be("UK");
    }
}
