using Fanex.Affiliate.Link.Commands;
using Fanex.Data.Repository;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class BaseNonQueryCommandTests
{
    // Create a concrete implementation for testing
    private class TestNonQueryCommand : BaseNonQueryCommand
    {
        public override string GetSettingKey() => "Test_Setting_Key";
        public override bool IsValid() => true;
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var command = new TestNonQueryCommand
        {
            ErrCode = 1,
            ErrMsg = "Test error message"
        };

        // Assert
        command.ErrCode.Should().Be(1);
        command.ErrMsg.Should().Be("Test error message");
    }

    [Fact]
    public void ShouldInheritFromNonQueryCommand()
    {
        // Arrange
        var command = new TestNonQueryCommand();

        // Assert
        command.Should().BeAssignableTo<NonQueryCommand>();
    }

    [Fact]
    public void ShouldBeAbstractClass()
    {
        // Act & Assert
        typeof(BaseNonQueryCommand).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void GetSettingKey_ShouldBeOverridable()
    {
        // Arrange
        var command = new TestNonQueryCommand();

        // Act
        var result = command.GetSettingKey();

        // Assert
        result.Should().Be("Test_Setting_Key");
    }

    [Fact]
    public void IsValid_ShouldBeOverridable()
    {
        // Arrange
        var command = new TestNonQueryCommand();

        // Act
        var result = command.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldHaveDefaultValues()
    {
        // Arrange
        var command = new TestNonQueryCommand();

        // Assert
        command.ErrCode.Should().Be(0);
        command.ErrMsg.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeMutable()
    {
        // Arrange
        var command = new TestNonQueryCommand();

        // Act
        command.ErrCode = 5;
        command.ErrMsg = "Updated error message";

        // Assert
        command.ErrCode.Should().Be(5);
        command.ErrMsg.Should().Be("Updated error message");
    }
}

