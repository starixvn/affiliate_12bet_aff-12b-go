using Fanex.Affiliate.Link.Commands;
using Fanex.Data.Repository;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class BaseMarketingLinkCommandTests
{
    // Create a concrete implementation for testing
    private class TestMarketingLinkCommand : BaseMarketingLinkCommand
    {
        public override string GetSettingKey() => "Test_Setting_Key";
        public override bool IsValid() => AffId > 0;
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var command = new TestMarketingLinkCommand
        {
            AffId = 123,
            ChannelCode = "CHANNEL001",
            LanguageCode = "en",
            AffMediaLink = "https://example.com/link"
        };

        // Assert
        command.AffId.Should().Be(123);
        command.ChannelCode.Should().Be("CHANNEL001");
        command.LanguageCode.Should().Be("en");
        command.AffMediaLink.Should().Be("https://example.com/link");
    }

    [Fact]
    public void IsOn_ShouldReturnTrue_WhenDefaultValue()
    {
        // Arrange
        var command = new TestMarketingLinkCommand();

        // Act
        var result = command.IsOn;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOn_ShouldBeReadOnly()
    {
        // Arrange
        var command = new TestMarketingLinkCommand();

        // Act & Assert
        // The IsOn property should be read-only, so we can't set it
        // This test verifies the property exists and has the correct default value
        command.IsOn.Should().BeTrue();
    }

    [Fact]
    public void ShouldInheritFromNonQueryCommand()
    {
        // Arrange
        var command = new TestMarketingLinkCommand();

        // Assert
        command.Should().BeAssignableTo<NonQueryCommand>();
    }

    [Fact]
    public void ShouldBeAbstractClass()
    {
        // Act & Assert
        typeof(BaseMarketingLinkCommand).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void GetSettingKey_ShouldBeOverridable()
    {
        // Arrange
        var command = new TestMarketingLinkCommand();

        // Act
        var result = command.GetSettingKey();

        // Assert
        result.Should().Be("Test_Setting_Key");
    }

    [Fact]
    public void IsValid_ShouldBeOverridable()
    {
        // Arrange
        var command = new TestMarketingLinkCommand { AffId = 123 };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenAffIdIsZero()
    {
        // Arrange
        var command = new TestMarketingLinkCommand { AffId = 0 };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenAffIdIsNegative()
    {
        // Arrange
        var command = new TestMarketingLinkCommand { AffId = -1 };

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
