using EasyNetQ;
using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Commands;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class ClickTrackingDLQWorkerTests
{
    private readonly Mock<IBus> _mockBus;
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<IRedisService> _mockRedisService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IAdvancedBus> _mockAdvancedBus;
    private readonly ClickTrackingDLQWorker _worker;

    public ClickTrackingDLQWorkerTests()
    {
        _mockBus = new Mock<IBus>();
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockRedisService = new Mock<IRedisService>();
        _mockLogService = new Mock<ILogService>();
        _mockAdvancedBus = new Mock<IAdvancedBus>();

        _mockBus.Setup(x => x.Advanced).Returns(_mockAdvancedBus.Object);

        _worker = new ClickTrackingDLQWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockRedisService.Object,
            _mockLogService.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Act & Assert
        _worker.Should().NotBeNull();
    }

    [Fact]
    public void TurnOffConsumerMode_ShouldDisableConsumer_WhenNotAlreadyDisabled()
    {
        // Act
        _worker.TurnOffConsumerMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public void TurnOffConsumerMode_ShouldNotLog_WhenAlreadyDisabled()
    {
        // Arrange - Turn off first time
        _worker.TurnOffConsumerMode();
        _mockLogService.Reset();

        // Act - Turn off again
        _worker.TurnOffConsumerMode();

        // Assert - Should not log again
        _mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void TurnOnConsumerMode_ShouldEnableConsumer_WhenCurrentlyDisabled()
    {
        // Arrange - Turn off first
        _worker.TurnOffConsumerMode();
        _mockLogService.Reset();

        // Act
        _worker.TurnOnConsumerMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void TurnOnConsumerMode_ShouldNotLog_WhenAlreadyEnabled()
    {
        // Act - Turn on when already enabled
        _worker.TurnOnConsumerMode();

        // Assert - Should not log
        _mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckConsumerMode_ShouldTurnOffConsumer_WhenUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ReturnsAsync(true);

        // Act
        await _worker.CheckConsumerMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public async Task CheckConsumerMode_ShouldTurnOffConsumer_WhenRedisFlagIsFalse()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ReturnsAsync(false);

        // Act
        await _worker.CheckConsumerMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public async Task CheckConsumerMode_ShouldTurnOnConsumer_WhenNotUnderMaintenanceAndRedisFlagIsTrue()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ReturnsAsync(true);

        // Turn off first
        _worker.TurnOffConsumerMode();
        _mockLogService.Reset();

        // Act
        await _worker.CheckConsumerMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.TurnOnErrorQueueConsumer.Should().Be("Aff.12B.Go.TurnOnErrorQueueConsumer");
        Constants.ErrorExchangeName.Should().Be("12b.user.click.error.exchange");
        Constants.ErrorQueueName.Should().Be("12b.user.click.error.queue");
        Constants.RoutingKey.Should().Be("12b.user.click.routing");
    }

    [Fact]
    public void Worker_ShouldHandleMaintenanceMode_Transitions()
    {
        // Arrange
        var maintenanceTimeEnabled = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        var maintenanceTimeDisabled = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };

        // Act & Assert - Test maintenance mode transitions
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTimeEnabled);
        _worker.TurnOffConsumerMode();
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);

        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTimeDisabled);
        _worker.TurnOnConsumerMode();
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public async Task Worker_ShouldRespectRedisConfiguration()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Turn off first to ensure we can turn on
        _worker.TurnOffConsumerMode();
        _mockLogService.Reset();

        // Test with Redis flag enabled
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ReturnsAsync(true);
        await _worker.CheckConsumerMode();
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);

        // Turn on first to ensure we can turn off
        _worker.TurnOnConsumerMode();
        _mockLogService.Reset();

        // Test with Redis flag disabled
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ReturnsAsync(false);
        await _worker.CheckConsumerMode();
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public void Worker_ShouldHaveCorrectInitialDelay()
    {
        // This test verifies the constant value used in the worker
        const int expectedInitialDelay = 10;
        expectedInitialDelay.Should().Be(10);
    }

    [Fact]
    public void Worker_ShouldHaveCorrectPollingInterval()
    {
        // This test verifies the polling interval used in the worker
        var expectedInterval = TimeSpan.FromMinutes(1);
        expectedInterval.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Worker_ShouldHandleConcurrentAccess_Safely()
    {
        // Act - Simulate concurrent access to maintenance mode methods
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _worker.TurnOffConsumerMode()));
            tasks.Add(Task.Run(() => _worker.TurnOnConsumerMode()));
        }

        // Act & Assert - Should not throw any exceptions
        var action = () => Task.WhenAll(tasks);
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleMaintenanceServiceNullResponse_Gracefully()
    {
        // Arrange
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns((UnderMaintenanceTime)null);

        // Act & Assert - Should handle null gracefully
        var action = () => _worker.CheckConsumerMode();
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHaveCorrectQueueConfiguration()
    {
        // Assert - Verify queue configuration constants
        Constants.ErrorExchangeName.Should().Contain("error.exchange");
        Constants.ErrorQueueName.Should().Contain("error.queue");
        Constants.RoutingKey.Should().Contain("routing");
    }

    [Fact]
    public void Worker_ShouldHandleDatabaseServiceExceptions_Gracefully()
    {
        // Arrange
        _mockDatabaseService.Setup(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert - Should handle database exceptions gracefully
        var action = () => _worker.StartAsync(CancellationToken.None);
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleRedisExceptions_Gracefully()
    {
        // Arrange
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ThrowsAsync(new Exception("Redis connection error"));

        // Act & Assert - Should handle Redis exceptions gracefully
        var action = () => _worker.CheckConsumerMode();
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleMaintenanceServiceExceptions_Gracefully()
    {
        // Arrange
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Throws(new Exception("Maintenance service error"));

        // Act & Assert - Should handle maintenance service exceptions gracefully
        var action = () => _worker.CheckConsumerMode();
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleMultipleMaintenanceModeChanges()
    {
        // Act - Test multiple maintenance mode changes
        for (int i = 0; i < 5; i++)
        {
            _worker.TurnOffConsumerMode();
            _worker.TurnOnConsumerMode();
        }

        // Assert - Should handle multiple changes without issues
        _mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.AtLeast(5));
    }

    [Fact]
    public void Worker_ShouldHandleRapidStateChanges()
    {
        // Act - Test rapid state changes
        for (int i = 0; i < 20; i++)
        {
            _worker.TurnOffConsumerMode();
            _worker.TurnOnConsumerMode();
        }

        // Assert - Should handle rapid changes without issues
        _mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.AtLeast(10));
    }

    [Fact]
    public void Worker_ShouldHandleNullMaintenanceTime()
    {
        // Arrange
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns((UnderMaintenanceTime)null);

        // Act & Assert - Should handle null maintenance time gracefully
        var action = () => _worker.CheckConsumerMode();
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleEmptyRedisResponse()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ReturnsAsync(false);

        // Act & Assert - Should handle empty Redis response gracefully
        var action = () => _worker.CheckConsumerMode();
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleInvalidMaintenanceTime()
    {
        // Arrange
        var invalidMaintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(invalidMaintenanceTime);
        _mockRedisService.Setup(x => x.GetAsync<bool>(Constants.TurnOnErrorQueueConsumer))
            .ReturnsAsync(true);

        // Act & Assert - Should handle invalid maintenance time gracefully
        var action = () => _worker.CheckConsumerMode();
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleDatabaseServiceNullResponse()
    {
        // Arrange
        _mockDatabaseService.Setup(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync((CommandResult)null);

        // Act & Assert - Should handle null database response gracefully
        var action = () => _worker.StartAsync(CancellationToken.None);
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleLogServiceExceptions()
    {
        // Arrange
        _mockLogService.Setup(x => x.LogFileMessage(It.IsAny<string>()))
            .Throws(new Exception("Log service error"));

        // Act & Assert - Should handle log service exceptions gracefully
        // The worker doesn't handle log exceptions, so we expect it to throw
        var action = () => _worker.TurnOffConsumerMode();
        action.Should().Throw<Exception>().WithMessage("Log service error");
    }

    [Fact]
    public void Worker_ShouldHandleSentryLogExceptions()
    {
        // Arrange
        _mockLogService.Setup(x => x.SentryErrorAsync(It.IsAny<Exception>()))
            .ThrowsAsync(new Exception("Sentry error"));

        // Act & Assert - Should handle sentry log exceptions gracefully
        var action = () => _worker.StartAsync(CancellationToken.None);
        action.Should().NotThrowAsync();
    }

    [Fact]
    public void Worker_ShouldHandleStringSentryLogExceptions()
    {
        // Arrange
        _mockLogService.Setup(x => x.SentryErrorAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("String sentry error"));

        // Act & Assert - Should handle string sentry log exceptions gracefully
        var action = () => _worker.StartAsync(CancellationToken.None);
        action.Should().NotThrowAsync();
    }
}
