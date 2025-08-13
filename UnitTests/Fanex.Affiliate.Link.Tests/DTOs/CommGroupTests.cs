using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class CommGroupTests
{
    [Fact]
    public void CommGroup_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var commGroup = new CommGroup
        {
            GroupName = "TestGroup"
        };

        // Assert
        commGroup.GroupName.Should().Be("TestGroup");
    }

    [Fact]
    public void CommGroup_ShouldHandleDefaultValues()
    {
        // Arrange
        var commGroup = new CommGroup();

        // Assert
        commGroup.GroupName.Should().BeNull();
    }

    [Fact]
    public void CommGroup_ShouldBeMutable()
    {
        // Arrange
        var commGroup = new CommGroup();

        // Act
        commGroup.GroupName = "UpdatedGroup";

        // Assert
        commGroup.GroupName.Should().Be("UpdatedGroup");
    }
}
