using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests;

public class ConstantsTests
{
    [Fact]
    public void QueueIdentify_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
    }

    [Fact]
    public void CacheRedisQueueName_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }

    [Fact]
    public void QueueIdentify_ShouldNotBeNullOrEmpty()
    {
        // Act & Assert
        Constants.QueueIdentify.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CacheRedisQueueName_ShouldNotBeNullOrEmpty()
    {
        // Act & Assert
        Constants.CacheRedisQueueName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void QueueIdentify_ShouldContainExpectedPattern()
    {
        // Act & Assert
        Constants.QueueIdentify.Should().Contain("12b");
        Constants.QueueIdentify.Should().Contain("user");
        Constants.QueueIdentify.Should().Contain("click");
    }

    [Fact]
    public void CacheRedisQueueName_ShouldContainExpectedPattern()
    {
        // Act & Assert
        Constants.CacheRedisQueueName.Should().Contain("Aff");
        Constants.CacheRedisQueueName.Should().Contain("12B");
        Constants.CacheRedisQueueName.Should().Contain("ClickQueues");
    }

    [Fact]
    public void QueueIdentify_ShouldBeImmutable()
    {
        // Act & Assert
        // Constants should be read-only and cannot be modified
        // This test verifies the value remains consistent
        var firstValue = Constants.QueueIdentify;
        var secondValue = Constants.QueueIdentify;

        firstValue.Should().Be(secondValue);
    }

    [Fact]
    public void CacheRedisQueueName_ShouldBeImmutable()
    {
        // Act & Assert
        // Constants should be read-only and cannot be modified
        // This test verifies the value remains consistent
        var firstValue = Constants.CacheRedisQueueName;
        var secondValue = Constants.CacheRedisQueueName;

        firstValue.Should().Be(secondValue);
    }

    [Fact]
    public void QueueIdentify_ShouldHaveCorrectLength()
    {
        // Act & Assert
        Constants.QueueIdentify.Should().HaveLength(14); // "12b.user.click" has 14 characters
    }

    [Fact]
    public void CacheRedisQueueName_ShouldHaveCorrectLength()
    {
        // Act & Assert
        Constants.CacheRedisQueueName.Should().HaveLength(22); // "Aff.12B.Go.ClickQueues" has 22 characters
    }

    [Fact]
    public void QueueIdentify_ShouldStartWithExpectedPrefix()
    {
        // Act & Assert
        Constants.QueueIdentify.Should().StartWith("12b");
    }

    [Fact]
    public void CacheRedisQueueName_ShouldStartWithExpectedPrefix()
    {
        // Act & Assert
        Constants.CacheRedisQueueName.Should().StartWith("Aff");
    }

    [Fact]
    public void QueueIdentify_ShouldEndWithExpectedSuffix()
    {
        // Act & Assert
        Constants.QueueIdentify.Should().EndWith("click");
    }

    [Fact]
    public void CacheRedisQueueName_ShouldEndWithExpectedSuffix()
    {
        // Act & Assert
        Constants.CacheRedisQueueName.Should().EndWith("ClickQueues");
    }

    [Fact]
    public void QueueIdentify_ShouldContainDots()
    {
        // Act & Assert
        Constants.QueueIdentify.Should().Contain(".");
    }

    [Fact]
    public void CacheRedisQueueName_ShouldContainDots()
    {
        // Act & Assert
        Constants.CacheRedisQueueName.Should().Contain(".");
    }

    [Fact]
    public void Constants_ShouldBeStaticClass()
    {
        // Act & Assert
        typeof(Constants).IsAbstract.Should().BeTrue();
        typeof(Constants).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void Constants_ShouldHavePublicConstants()
    {
        // Act & Assert
        var queueIdentifyField = typeof(Constants).GetField("QueueIdentify");
        var cacheRedisQueueNameField = typeof(Constants).GetField("CacheRedisQueueName");

        queueIdentifyField.Should().NotBeNull();
        cacheRedisQueueNameField.Should().NotBeNull();

        queueIdentifyField!.IsPublic.Should().BeTrue();
        cacheRedisQueueNameField!.IsPublic.Should().BeTrue();

        queueIdentifyField.IsStatic.Should().BeTrue();
        cacheRedisQueueNameField.IsStatic.Should().BeTrue();

        queueIdentifyField.IsLiteral.Should().BeTrue();
        cacheRedisQueueNameField.IsLiteral.Should().BeTrue();
    }
}
