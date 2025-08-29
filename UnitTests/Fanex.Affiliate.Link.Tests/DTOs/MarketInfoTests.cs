using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class MarketInfoTests
{
    [Fact]
    public void MarketInfo_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var marketInfo = new MarketInfo
        {
            MarketId = "1",
            ShortedCode = "TEST"
        };

        // Assert
        marketInfo.MarketId.Should().Be("1");
        marketInfo.ShortedCode.Should().Be("TEST");
    }

    [Fact]
    public void MarketInfo_ShouldHandleDefaultValues()
    {
        // Arrange
        var marketInfo = new MarketInfo();

        // Assert
        marketInfo.MarketId.Should().BeNull();
        marketInfo.ShortedCode.Should().BeNull();
    }

    [Fact]
    public void MarketInfo_ShouldBeMutable()
    {
        // Arrange
        var marketInfo = new MarketInfo();

        // Act
        marketInfo.MarketId = "2";
        marketInfo.ShortedCode = "UPDATED";

        // Assert
        marketInfo.MarketId.Should().Be("2");
        marketInfo.ShortedCode.Should().Be("UPDATED");
    }
}
