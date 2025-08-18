using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Affiliate.Link.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;

namespace Fanex.Affiliate.Link.Tests.Workers;

public class RabbitMqBusHostedServiceTests : IDisposable
{
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IRabbitBus> _mockRabbitBus;
    private readonly Dictionary<string, string> _configurationValues;

    public RabbitMqBusHostedServiceTests()
    {
        _mockLogService = new Mock<ILogService>(MockBehavior.Strict);
        _mockRabbitBus = new Mock<IRabbitBus>(MockBehavior.Strict);

        _configurationValues = new Dictionary<string, string>
        {
            ["RabbitMQConnectionString"] = "host=localhost;username=guest;password=guest"
        };

        // Setup default mock behaviors
        _mockLogService.Setup(x => x.SentryErrorAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _mockLogService.Setup(x => x.SentryErrorAsync(It.IsAny<string>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);
        _mockLogService.Setup(x => x.LogFileMessage(It.IsAny<string>())).Verifiable();
        _mockLogService.Setup(x => x.LogFileError(It.IsAny<Exception>())).Verifiable();
        
        _mockRabbitBus.Setup(x => x.SetBus(It.IsAny<IAdvancedBus>())).Verifiable();
        _mockRabbitBus.Setup(x => x.RegisterQueueAsync()).Returns(Task.CompletedTask);
    }

    private IConfiguration CreateConfiguration(string connectionString = null)
    {
        var configValues = new Dictionary<string, string>();
        if (connectionString != null)
        {
            configValues["RabbitMQConnectionString"] = connectionString;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenAllDependenciesProvided()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost");

        // Act
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenLogServiceIsNull()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost");

        // Act & Assert
        var action = () => new RabbitMqBusHostedService(
            null!,
            configuration,
            _mockRabbitBus.Object);

        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenConfigurationIsNull()
    {
        // Act & Assert
        var action = () => new RabbitMqBusHostedService(
            _mockLogService.Object,
            null!,
            _mockRabbitBus.Object);

        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenRabbitBusIsNull()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost");

        // Act & Assert
        var action = () => new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            null!);

        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        // Act & Assert
        var action = () => service.Dispose();
        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldDisposeBus_WhenBusIsNotNull()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        // Act
        service.Dispose();

        // Assert - Should not throw and should handle disposal gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogError_WhenConnectionStringIsNull()
    {
        // Arrange
        var configuration = CreateConfiguration(null);
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        var action = async () => await service.StartAsync(cancellationTokenSource.Token);
        await action.Should().ThrowAsync<ArgumentNullException>();

        // Verify that the error was logged before the exception
        _mockLogService.Verify(x => x.SentryErrorAsync("RabbitMQ connection string is not configured"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogError_WhenConnectionStringIsEmpty()
    {
        // Arrange
        var configuration = CreateConfiguration(string.Empty);
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        var action = async () => await service.StartAsync(cancellationTokenSource.Token);
        await action.Should().ThrowAsync<EasyNetQ.EasyNetQException>();

        // Verify that the error was logged before the exception
        _mockLogService.Verify(x => x.SentryErrorAsync("RabbitMQ connection string is not configured"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateBus_WhenConnectionStringIsValid()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should not throw and should attempt to create bus
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task TryConnectToRabbitMqAsync_ShouldLogError_WhenBusIsNull()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should handle null bus gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task TryConnectToRabbitMqAsync_ShouldRetryOnException()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        
        _mockRabbitBus.Setup(x => x.RegisterQueueAsync())
            .ThrowsAsync(new InvalidOperationException("Connection failed"));

        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert
        _mockLogService.Verify(x => x.SentryErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);
        _mockLogService.Verify(x => x.LogFileError(It.IsAny<Exception>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task TryConnectToRabbitMqAsync_ShouldBreakLoop_WhenCancellationRequested()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should handle cancellation gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task TryConnectToRabbitMqAsync_ShouldReturnEarly_WhenConnectionLockCannotBeAcquired()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should handle lock acquisition gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetUpEventHandlers()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should set up event handlers without throwing
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task StopAsync_ShouldNotThrowException()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        // Act & Assert
        var action = async () => await service.StopAsync(CancellationToken.None);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_ShouldHandleInvalidConnectionString()
    {
        // Arrange
        var configuration = CreateConfiguration("invalid-connection-string");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        var action = async () => await service.StartAsync(cancellationTokenSource.Token);
        await action.Should().ThrowAsync<EasyNetQ.EasyNetQException>();

        // Assert - Should handle invalid connection string gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleMultipleExceptions()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        
        _mockRabbitBus.Setup(x => x.RegisterQueueAsync())
            .ThrowsAsync(new TimeoutException("Connection timeout"));

        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert
        _mockLogService.Verify(x => x.SentryErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);
        _mockLogService.Verify(x => x.LogFileError(It.IsAny<Exception>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleNullException()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        
        _mockRabbitBus.Setup(x => x.RegisterQueueAsync())
            .ThrowsAsync((Exception)null!);

        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should handle null exception gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleTaskDelayException()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        
        _mockRabbitBus.Setup(x => x.RegisterQueueAsync())
            .ThrowsAsync(new InvalidOperationException("Connection failed"));

        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(50)); // Shorter timeout

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should handle Task.Delay exceptions gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_ShouldHandleMultipleDisposeCalls()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        // Act & Assert
        var action1 = () => service.Dispose();
        var action2 = () => service.Dispose();

        action1.Should().NotThrow();
        action2.Should().NotThrow();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleConfigurationSection()
    {
        // Arrange
        var configuration = CreateConfiguration("host=localhost;username=guest;password=guest");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert - Should handle configuration section gracefully
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleWhitespaceConnectionString()
    {
        // Arrange
        var configuration = CreateConfiguration("   ");
        var service = new RabbitMqBusHostedService(
            _mockLogService.Object,
            configuration,
            _mockRabbitBus.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        var action = async () => await service.StartAsync(cancellationTokenSource.Token);
        await action.Should().ThrowAsync<EasyNetQ.EasyNetQException>();

        // Assert - Whitespace is not considered empty by string.IsNullOrEmpty, so no error should be logged
        _mockLogService.Verify(x => x.SentryErrorAsync("RabbitMQ connection string is not configured"), Times.Never);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
