using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class BaseNonQueryCommandEnhancedTests
{
    [Fact]
    public void BaseNonQueryCommand_ShouldBeAbstractClass()
    {
        // Act & Assert
        typeof(BaseNonQueryCommand).IsAbstract.Should().BeTrue();
        typeof(BaseNonQueryCommand).IsClass.Should().BeTrue();
    }

    [Fact]
    public void BaseNonQueryCommand_ShouldInheritFromCorrectBase()
    {
        // Act & Assert
        typeof(BaseNonQueryCommand).BaseType?.Name.Should().Be("NonQueryCommand");
    }
}

