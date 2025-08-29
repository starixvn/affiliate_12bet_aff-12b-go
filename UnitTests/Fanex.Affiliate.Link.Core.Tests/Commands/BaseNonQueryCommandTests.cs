using Fanex.Affiliate.Link.Core.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests.Commands;

public class BaseNonQueryCommandTests
{
    [Fact]
    public void BaseNonQueryCommand_ShouldSetAndGetErrCode()
    {
        // Arrange
        var command = new TestBaseNonQueryCommand
        {
            ErrCode = 1001,
            ErrMsg = "Test error message"
        };

        // Act & Assert
        command.ErrCode.Should().Be(1001);
    }

    [Fact]
    public void BaseNonQueryCommand_ShouldSetAndGetErrMsg()
    {
        // Arrange
        var command = new TestBaseNonQueryCommand
        {
            ErrCode = 1001,
            ErrMsg = "Test error message"
        };

        // Act & Assert
        command.ErrMsg.Should().Be("Test error message");
    }

    [Fact]
    public void BaseNonQueryCommand_ShouldAllowZeroErrCode()
    {
        // Arrange
        var command = new TestBaseNonQueryCommand
        {
            ErrCode = 0,
            ErrMsg = "Test error message"
        };

        // Act & Assert
        command.ErrCode.Should().Be(0);
    }

    [Fact]
    public void BaseNonQueryCommand_ShouldAllowNegativeErrCode()
    {
        // Arrange
        var command = new TestBaseNonQueryCommand
        {
            ErrCode = -1,
            ErrMsg = "Test error message"
        };

        // Act & Assert
        command.ErrCode.Should().Be(-1);
    }

    [Fact]
    public void BaseNonQueryCommand_ShouldAllowEmptyErrMsg()
    {
        // Arrange
        var command = new TestBaseNonQueryCommand
        {
            ErrCode = 1001,
            ErrMsg = ""
        };

        // Act & Assert
        command.ErrMsg.Should().Be("");
    }

    [Fact]
    public void BaseNonQueryCommand_ShouldAllowNullErrMsg()
    {
        // Arrange
        var command = new TestBaseNonQueryCommand
        {
            ErrCode = 1001,
            ErrMsg = null!
        };

        // Act & Assert
        command.ErrMsg.Should().BeNull();
    }

    [Fact]
    public void BaseNonQueryCommand_ShouldInheritFromNonQueryCommand()
    {
        // Arrange
        var command = new TestBaseNonQueryCommand
        {
            ErrCode = 1001,
            ErrMsg = "Test error message"
        };

        // Act & Assert
        command.Should().BeAssignableTo<Fanex.Data.Repository.NonQueryCommand>();
    }

    // Test implementation of abstract class
    private class TestBaseNonQueryCommand : BaseNonQueryCommand
    {
        public override string GetSettingKey() => "TestSettingKey";
        public override bool IsValid() => true;
    }
}
