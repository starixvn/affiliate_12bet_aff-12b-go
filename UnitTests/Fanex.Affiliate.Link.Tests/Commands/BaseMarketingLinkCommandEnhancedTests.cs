using Fanex.Affiliate.Link.Commands;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Commands;

public class BaseMarketingLinkCommandEnhancedTests
{
    [Fact]
    public void BaseMarketingLinkCommand_ShouldBeAbstractClass()
    {
        // Act & Assert
        typeof(BaseMarketingLinkCommand).IsAbstract.Should().BeTrue();
        typeof(BaseMarketingLinkCommand).IsClass.Should().BeTrue();
    }

    [Fact]
    public void BaseMarketingLinkCommand_ShouldInheritFromCorrectBase()
    {
        // Act & Assert
        typeof(BaseMarketingLinkCommand).BaseType?.Name.Should().Be("NonQueryCommand");
    }
}

