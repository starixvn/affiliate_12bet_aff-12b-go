using Fanex.Affiliate.Link.Core;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class ConstantsTests
{
    [Fact]
    public void QueueIdentify_ShouldHaveCorrectValue()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
    }

    [Fact]
    public void CacheRedisQueueName_ShouldHaveCorrectValue()
    {
        // Assert
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }

    [Fact]
    public void Constants_ShouldNotBeNull()
    {
        // Assert
        Constants.QueueIdentify.Should().NotBeNull();
        Constants.CacheRedisQueueName.Should().NotBeNull();
    }

    [Fact]
    public void Constants_ShouldNotBeEmpty()
    {
        // Assert
        Constants.QueueIdentify.Should().NotBeEmpty();
        Constants.CacheRedisQueueName.Should().NotBeEmpty();
    }
}
