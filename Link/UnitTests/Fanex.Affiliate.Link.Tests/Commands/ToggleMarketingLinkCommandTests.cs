using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class ToggleMarketingLinkCommandTests
{
    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var command = new ToggleMarketingLinkCommand();

        // Act
        var result = command.GetSettingKey();

        // Assert
        result.Should().Be("Aff_User_AffMediaLink_ToggleChannel");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValid_ShouldReturnCorrectResult_BasedOnAffId(int affId, bool expectedResult)
    {
        // Arrange
        var command = new ToggleMarketingLinkCommand
        {
            AffId = affId,
            ErrCode = 0,
            ErrMsg = "Success"
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
        var command = new ToggleMarketingLinkCommand
        {
            AffId = 123,
            ErrCode = 1,
            ErrMsg = "Error occurred"
        };

        // Assert
        command.AffId.Should().Be(123);
        command.ErrCode.Should().Be(1);
        command.ErrMsg.Should().Be("Error occurred");
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenAffIdIsZero()
    {
        // Arrange
        var command = new ToggleMarketingLinkCommand
        {
            AffId = 0,
            ErrCode = 0,
            ErrMsg = "Success"
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
        var command = new ToggleMarketingLinkCommand
        {
            AffId = -1,
            ErrCode = 0,
            ErrMsg = "Success"
        };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenAffIdIsPositive()
    {
        // Arrange
        var command = new ToggleMarketingLinkCommand
        {
            AffId = 1,
            ErrCode = 0,
            ErrMsg = "Success"
        };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeTrue();
    }
}
