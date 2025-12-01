using EasyNetQ;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using System.Reflection;
using Xunit;
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

    [Fact]
    public async Task PublishAsync_ShouldHandleNullModel()
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
        var result = await _rabbitBus.PublishAsync((TestModel)null!);

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleDifferentExceptionTypes()
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
            .ThrowsAsync(new TimeoutException("connection timeout"));

        // Act
        var result = await _rabbitBus.PublishAsync(new TestModel { Id = 1, Name = "timeout" });

        // Assert
        result.Should().BeFalse();
        _mockLogService.Verify(x => x.SentryErrorAsync(It.Is<string>(s => s.Contains("Error publishing object")), It.IsAny<Exception>()), Times.Once);
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Message queued for later publishing"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleAggregateException()
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
            .ThrowsAsync(new AggregateException(new Exception("inner exception")));

        // Act
        var result = await _rabbitBus.PublishAsync(new TestModel { Id = 1, Name = "aggregate" });

        // Assert
        result.Should().BeFalse();
        _mockLogService.Verify(x => x.SentryErrorAsync(It.Is<string>(s => s.Contains("Error publishing object")), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnFalse_WhenExchangeIsNullAfterException()
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

        // Act - First call throws and resets exchange to null
        var result1 = await _rabbitBus.PublishAsync(new TestModel { Id = 1, Name = "fail" });

        // Second call should return false because exchange is null and RegisterQueueAsync will fail
        var result2 = await _rabbitBus.PublishAsync(new TestModel { Id = 2, Name = "fail again" });

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleComplexModel()
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
                It.IsAny<IMessage<ComplexTestModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var complexModel = new ComplexTestModel
        {
            Id = 1,
            Name = "Complex",
            Data = new { Value = 123, Text = "test" },
            List = new[] { "item1", "item2" }
        };

        // Act
        var result = await _rabbitBus.PublishAsync(complexModel);

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleEmptyStringModel()
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
                It.IsAny<IMessage<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _rabbitBus.PublishAsync("");

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleWhitespaceStringModel()
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
                It.IsAny<IMessage<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _rabbitBus.PublishAsync("   ");

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleLargeModel()
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
                It.IsAny<IMessage<LargeTestModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var largeModel = new LargeTestModel
        {
            Id = 1,
            Data = new string('x', 10000) // Large string
        };

        // Act
        var result = await _rabbitBus.PublishAsync(largeModel);

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleModelWithSpecialCharacters()
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

        var modelWithSpecialChars = new TestModel
        {
            Id = 1,
            Name = "Test with \"quotes\" and 'apostrophes' and \n newlines"
        };

        // Act
        var result = await _rabbitBus.PublishAsync(modelWithSpecialChars);

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleModelWithUnicodeCharacters()
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

        var modelWithUnicode = new TestModel
        {
            Id = 1,
            Name = "Test with unicode: 中文, español, français, русский"
        };

        // Act
        var result = await _rabbitBus.PublishAsync(modelWithUnicode);

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleJsonSerializationException()
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
            .ThrowsAsync(new Newtonsoft.Json.JsonSerializationException("Serialization failed"));

        // Act
        var result = await _rabbitBus.PublishAsync(new TestModel { Id = 1, Name = "serialization error" });

        // Assert
        result.Should().BeFalse();
        _mockLogService.Verify(x => x.SentryErrorAsync(It.Is<string>(s => s.Contains("Error publishing object")), It.IsAny<Exception>()), Times.Once);
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Message queued for later publishing"))), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleModelWithDateTime()
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
                It.IsAny<IMessage<DateTimeTestModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var dateTimeModel = new DateTimeTestModel
        {
            Id = 1,
            Name = "DateTime Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.Now
        };

        // Act
        var result = await _rabbitBus.PublishAsync(dateTimeModel);

        // Assert
        result.Should().BeTrue();
        _mockLogService.Verify(x => x.LogFileMessage(It.Is<string>(s => s.Contains("Published model"))), Times.Once);
    }

    // ===== NEW HEALTH CHECK TESTS =====

    [Fact]
    public void IsConnected_ShouldReturnFalse_WhenAdvancedBusIsNull()
    {
        // Act
        var result = _rabbitBus.IsConnected;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsConnected_ShouldReturnTrue_WhenAdvancedBusIsConnected()
    {
        // Arrange
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(true);
        _rabbitBus.SetBus(_mockAdvancedBus.Object);

        // Act
        var result = _rabbitBus.IsConnected;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsConnected_ShouldReturnFalse_WhenAdvancedBusIsNotConnected()
    {
        // Arrange
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(false);
        _rabbitBus.SetBus(_mockAdvancedBus.Object);

        // Act
        var result = _rabbitBus.IsConnected;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetConnectionInfoAsync_ShouldReturnBasicInfo_WhenAdvancedBusIsNull()
    {
        // Act
        var result = await _rabbitBus.GetConnectionInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result["IsConnected"].Should().Be(false);
        result["HasAdvancedBus"].Should().Be(false);
        result["HasExchange"].Should().Be(false);
        result.Should().ContainKey("LastCheck");
    }

    [Fact]
    public async Task GetConnectionInfoAsync_ShouldReturnDetailedInfo_WhenAdvancedBusIsConnected()
    {
        // Arrange
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(true);
        _rabbitBus.SetBus(_mockAdvancedBus.Object);

        // Act
        var result = await _rabbitBus.GetConnectionInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result["IsConnected"].Should().Be(true);
        result["HasAdvancedBus"].Should().Be(true);
        result["ConnectionState"].Should().Be("Connected");
        result["ConnectionActive"].Should().Be(true);
        result["ConnectionType"].Should().Be("EasyNetQ.IAdvancedBus");
    }

    [Fact]
    public async Task GetConnectionInfoAsync_ShouldReturnDisconnectedInfo_WhenAdvancedBusIsNotConnected()
    {
        // Arrange
        _mockAdvancedBus.SetupGet(x => x.IsConnected).Returns(false);
        _rabbitBus.SetBus(_mockAdvancedBus.Object);

        // Act
        var result = await _rabbitBus.GetConnectionInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result["IsConnected"].Should().Be(false);
        result["HasAdvancedBus"].Should().Be(true);
        result["ConnectionState"].Should().Be("Disconnected");
        result.Should().NotContainKey("ConnectionActive");
    }

    [Fact]
    public async Task GetConnectionInfoAsync_ShouldHandleException_AndLogError()
    {
        // Arrange - Setup a scenario where IsConnected throws an exception
        _mockAdvancedBus.SetupSequence(x => x.IsConnected)
            .Returns(false) // First call in IsConnected property works
            .Throws(new Exception("Connection info error")); // Second call in GetConnectionInfoAsync throws
        _rabbitBus.SetBus(_mockAdvancedBus.Object);

        // Act
        var result = await _rabbitBus.GetConnectionInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result["Error"].Should().Be("Connection info error");
        _mockLogService.Verify(x => x.SentryErrorAsync("Failed to get RabbitMQ connection info", It.IsAny<Exception>()), Times.Once);
    }

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class ComplexTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public object Data { get; set; } = new();
        public string[] List { get; set; } = Array.Empty<string>();
    }

    private class LargeTestModel
    {
        public int Id { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    private class DateTimeTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
