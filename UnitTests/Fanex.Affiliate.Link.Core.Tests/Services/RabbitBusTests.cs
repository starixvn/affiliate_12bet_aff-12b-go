using EasyNetQ;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core.Services;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using CoreRabbitBus = Fanex.Affiliate.Link.Core.Services.RabbitBus;

namespace Fanex.Affiliate.Link.Core.Tests.Services;

public class RabbitBusTests
{
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IAdvancedBus> _mockAdvancedBus;
    private readonly CoreRabbitBus _rabbitBus;

    public RabbitBusTests()
    {
        _mockLogService = new Mock<ILogService>(MockBehavior.Strict);
        _mockAdvancedBus = new Mock<IAdvancedBus>(MockBehavior.Strict);

        // Common log setups
        _mockLogService.Setup(x => x.LogFileMessage(It.IsAny<string>())).Verifiable();
        _mockLogService.Setup(x => x.SentryErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()))
            .Returns(Task.CompletedTask);

        _rabbitBus = new CoreRabbitBus(_mockLogService.Object);
    }

    private static void PrimeExchange(object rabbitBus, string name = "affiliate.exchange")
    {
        var field = typeof(CoreRabbitBus).GetField("exchange", BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(rabbitBus, new Exchange(name));
    }

    [Fact]
    public async Task SetBus_ShouldSetAdvancedBus()
    {
        _rabbitBus.SetBus(_mockAdvancedBus.Object);
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(false);
        var result = await _rabbitBus.PublishAsync(new TestModel());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnFalse_WhenBusIsNull()
    {
        var result = await _rabbitBus.PublishAsync(new TestModel());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnFalse_WhenBusIsNotConnected()
    {
        _rabbitBus.SetBus(_mockAdvancedBus.Object);
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(false);

        var result = await _rabbitBus.PublishAsync(new TestModel());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAsync_ShouldPublish_WhenExchangePrimed()
    {
        // Arrange
        _rabbitBus.SetBus(_mockAdvancedBus.Object);
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(true);
        PrimeExchange(_rabbitBus);

        _mockAdvancedBus
            .Setup(x => x.PublishAsync(
                It.IsAny<Exchange>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IMessage<TestModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _rabbitBus.PublishAsync(new TestModel { Id = 1, Name = "ok" });

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandlePublishException_WhenExchangePrimed()
    {
        // Arrange
        _rabbitBus.SetBus(_mockAdvancedBus.Object);
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(true);
        PrimeExchange(_rabbitBus);

        _mockAdvancedBus
            .Setup(x => x.PublishAsync(
                It.IsAny<Exchange>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IMessage<TestModel>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("publish failed"));

        // Act
        var result1 = await _rabbitBus.PublishAsync(new TestModel { Id = 1, Name = "fail" });

        // Assert
        result1.Should().BeFalse();
        _mockLogService.Verify(x => x.SentryErrorAsync(It.Is<string>(s => s.Contains("Error publishing object")), It.IsAny<Exception>()), Times.Once);
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Message queued for later publishing"))), Times.Once);

        // Prime again for success path
        PrimeExchange(_rabbitBus);
        _mockAdvancedBus
            .Setup(x => x.PublishAsync(
                It.IsAny<Exchange>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IMessage<TestModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result2 = await _rabbitBus.PublishAsync(new TestModel { Id = 2, Name = "ok" });
        result2.Should().BeTrue();
    }

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
