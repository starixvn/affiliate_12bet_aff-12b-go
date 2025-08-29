using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class AffiliateInfoEnhancedTests
{
    [Fact]
    public void AffiliateInfo_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo();

        // Assert
        affiliateInfo.Should().NotBeNull();
        affiliateInfo.AffId.Should().Be(0);
        affiliateInfo.AffCode.Should().BeNull();
    }

    [Fact]
    public void AffiliateInfo_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var expectedAffId = 12345;
        var expectedAffCode = "TEST123";

        // Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = expectedAffId,
            AffCode = expectedAffCode
        };

        // Assert
        affiliateInfo.AffId.Should().Be(expectedAffId);
        affiliateInfo.AffCode.Should().Be(expectedAffCode);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(123, "")]
    [InlineData(456, "ABC123")]
    [InlineData(-1, "NEGATIVE")]
    [InlineData(999999, "LARGE_CODE")]
    public void AffiliateInfo_ShouldAcceptVariousValues(int affId, string affCode)
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = affId,
            AffCode = affCode
        };

        // Assert
        affiliateInfo.AffId.Should().Be(affId);
        affiliateInfo.AffCode.Should().Be(affCode);
    }

    [Fact]
    public void AffiliateInfo_Properties_ShouldBeSettableIndependently()
    {
        // Arrange
        var affiliateInfo = new AffiliateInfo();

        // Act & Assert
        affiliateInfo.AffId = 100;
        affiliateInfo.AffId.Should().Be(100);
        affiliateInfo.AffCode.Should().BeNull();

        affiliateInfo.AffCode = "CODE100";
        affiliateInfo.AffCode.Should().Be("CODE100");
        affiliateInfo.AffId.Should().Be(100);
    }

    [Fact]
    public void AffiliateInfo_ShouldSupportObjectInitializer()
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = 789,
            AffCode = "INIT789"
        };

        // Assert
        affiliateInfo.AffId.Should().Be(789);
        affiliateInfo.AffCode.Should().Be("INIT789");
    }

    [Fact]
    public void AffiliateInfo_ShouldHandleNullAffCode()
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = 123,
            AffCode = null
        };

        // Assert
        affiliateInfo.AffId.Should().Be(123);
        affiliateInfo.AffCode.Should().BeNull();
    }

    [Fact]
    public void AffiliateInfo_ShouldHandleEmptyAffCode()
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = 123,
            AffCode = ""
        };

        // Assert
        affiliateInfo.AffId.Should().Be(123);
        affiliateInfo.AffCode.Should().Be("");
    }

    [Fact]
    public void AffiliateInfo_ShouldHandleWhitespaceAffCode()
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = 123,
            AffCode = "   "
        };

        // Assert
        affiliateInfo.AffId.Should().Be(123);
        affiliateInfo.AffCode.Should().Be("   ");
    }

    [Fact]
    public void AffiliateInfo_ShouldHandleZeroAffId()
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = 0,
            AffCode = "ZERO"
        };

        // Assert
        affiliateInfo.AffId.Should().Be(0);
        affiliateInfo.AffCode.Should().Be("ZERO");
    }

    [Fact]
    public void AffiliateInfo_ShouldHandleNegativeAffId()
    {
        // Arrange & Act
        var affiliateInfo = new AffiliateInfo
        {
            AffId = -1,
            AffCode = "NEGATIVE"
        };

        // Assert
        affiliateInfo.AffId.Should().Be(-1);
        affiliateInfo.AffCode.Should().Be("NEGATIVE");
    }
}
