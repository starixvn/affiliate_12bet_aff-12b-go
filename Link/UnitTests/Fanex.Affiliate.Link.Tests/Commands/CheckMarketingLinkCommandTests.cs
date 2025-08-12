using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class CheckMarketingLinkCommandTests
{
    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var command = new CheckMarketingLinkCommand();

        // Act
        var result = command.GetSettingKey();

        // Assert
        result.Should().Be("Aff_User_AffMediaLink_CheckExistedTracking");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValid_ShouldReturnCorrectResult_BasedOnAffId(int affId, bool expectedResult)
    {
        // Arrange
        var command = new CheckMarketingLinkCommand
        {
            AffId = affId,
            AffDomain = "test.com",
            IsExistedTracking = true,
            IsValidDomain = true
        };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var command = new CheckMarketingLinkCommand
        {
            AffId = 123,
            AffDomain = "test.com",
            IsExistedTracking = true,
            IsValidDomain = false
        };

        // Assert
        command.AffId.Should().Be(123);
        command.AffDomain.Should().Be("test.com");
        command.IsExistedTracking.Should().BeTrue();
        command.IsValidDomain.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenAffIdIsZero()
    {
        // Arrange
        var command = new CheckMarketingLinkCommand
        {
            AffId = 0,
            AffDomain = "test.com",
            IsExistedTracking = true,
            IsValidDomain = true
        };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenAffIdIsNegative()
    {
        // Arrange
        var command = new CheckMarketingLinkCommand
        {
            AffId = -1,
            AffDomain = "test.com",
            IsExistedTracking = true,
            IsValidDomain = true
        };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
