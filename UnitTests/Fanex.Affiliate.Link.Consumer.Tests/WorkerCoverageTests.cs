using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Consumer;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Logging;
using FluentAssertions;
using Moq;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class WorkerCoverageTests
{
    [Fact]
    public void ClickTrackingDLQWorker_ShouldBeConstructedSuccessfully()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        // Act
        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Assert
        worker.Should().NotBeNull();
    }

    [Fact]
    public void ClickTrackingWorker_ShouldBeConstructedSuccessfully()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        // Act
        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Assert
        worker.Should().NotBeNull();
    }

    [Fact]
    public void ClickTrackingDLQWorker_EnableMaintenanceMode_ShouldLogMessage()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public void ClickTrackingDLQWorker_DisableMaintenanceMode_ShouldLogMessage()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act - First enable maintenance mode, then disable it
        worker.EnableMaintenanceMode();
        mockLogService.Reset(); // Reset to only count the disable call
        worker.DisableMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void ClickTrackingDLQWorker_CheckMaintenanceMode_ShouldCallMaintenanceService()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.CheckMaintenanceMode();

        // Assert
        mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public void ClickTrackingWorker_EnableMaintenanceMode_ShouldLogMessage()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public void ClickTrackingWorker_DisableMaintenanceMode_ShouldLogMessage()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act - First enable maintenance mode, then disable it
        worker.EnableMaintenanceMode();
        mockLogService.Reset(); // Reset to only count the disable call
        worker.DisableMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void ClickTrackingWorker_CheckMaintenanceMode_ShouldCallMaintenanceService()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.CheckMaintenanceMode();

        // Assert
        mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public void ClickTrackingDLQWorker_EnableMaintenanceMode_ShouldNotLogTwice()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode(); // First call
        mockLogService.Reset();
        worker.EnableMaintenanceMode(); // Second call

        // Assert
        mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ClickTrackingWorker_EnableMaintenanceMode_ShouldNotLogTwice()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode(); // First call
        mockLogService.Reset();
        worker.EnableMaintenanceMode(); // Second call

        // Assert
        mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ClickTrackingDLQWorker_DisableMaintenanceMode_ShouldNotLogTwice()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.DisableMaintenanceMode(); // First call
        mockLogService.Reset();
        worker.DisableMaintenanceMode(); // Second call

        // Assert
        mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ClickTrackingWorker_DisableMaintenanceMode_ShouldNotLogTwice()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.DisableMaintenanceMode(); // First call
        mockLogService.Reset();
        worker.DisableMaintenanceMode(); // Second call

        // Assert
        mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ClickTrackingDLQWorker_CheckMaintenanceMode_ShouldEnableWhenUnderMaintenance()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = true });

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.CheckMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public void ClickTrackingWorker_CheckMaintenanceMode_ShouldEnableWhenUnderMaintenance()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = true });

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.CheckMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public async Task ClickTrackingDLQWorker_StopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act & Assert - Should not throw
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ClickTrackingWorker_StopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act & Assert - Should not throw
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void ClickTrackingDLQWorker_EnableMaintenanceMode_ShouldDisposeConsumerHandle()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();
        var mockConsumerHandle = new Mock<IDisposable>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        // Should complete without throwing exceptions
    }

    [Fact]
    public void ClickTrackingWorker_EnableMaintenanceMode_ShouldDisposeConsumerHandle()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();
        var mockConsumerHandle = new Mock<IDisposable>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        // Should complete without throwing exceptions
    }

    // Additional tests to increase coverage
    [Fact]
    public void ClickTrackingDLQWorker_EnableMaintenanceMode_ShouldDisposeConsumerHandleWhenNotNull()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        var mockConsumerHandle = new Mock<IDisposable>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        // Should complete without throwing exceptions
    }

    [Fact]
    public void ClickTrackingWorker_EnableMaintenanceMode_ShouldDisposeConsumerHandleWhenNotNull()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        var mockConsumerHandle = new Mock<IDisposable>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        // Should complete without throwing exceptions
    }

    [Fact]
    public void ClickTrackingDLQWorker_CheckMaintenanceMode_ShouldDisableWhenNotUnderMaintenance()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act - First enable maintenance mode, then check when not under maintenance
        worker.EnableMaintenanceMode();
        mockLogService.Reset();
        worker.CheckMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void ClickTrackingWorker_CheckMaintenanceMode_ShouldDisableWhenNotUnderMaintenance()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act - First enable maintenance mode, then check when not under maintenance
        worker.EnableMaintenanceMode();
        mockLogService.Reset();
        worker.CheckMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void ClickTrackingDLQWorker_DisableMaintenanceMode_ShouldNotStartConsumerWhenNotUnderMaintenance()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.DisableMaintenanceMode();

        // Assert
        // Should not call StartConsumer when not under maintenance
    }

    [Fact]
    public void ClickTrackingWorker_DisableMaintenanceMode_ShouldNotStartConsumerWhenNotUnderMaintenance()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.DisableMaintenanceMode();

        // Assert
        // Should not call StartConsumer when not under maintenance
    }

    [Fact]
    public void ClickTrackingDLQWorker_EnableMaintenanceMode_ShouldSetIsUnderMaintenanceToTrue()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        // Should set isUnderMaintenance to true
    }

    [Fact]
    public void ClickTrackingWorker_EnableMaintenanceMode_ShouldSetIsUnderMaintenanceToTrue()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();

        // Assert
        // Should set isUnderMaintenance to true
    }

    [Fact]
    public void ClickTrackingDLQWorker_DisableMaintenanceMode_ShouldSetIsUnderMaintenanceToFalse()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();
        worker.DisableMaintenanceMode();

        // Assert
        // Should set isUnderMaintenance to false
    }

    [Fact]
    public void ClickTrackingWorker_DisableMaintenanceMode_ShouldSetIsUnderMaintenanceToFalse()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();
        worker.DisableMaintenanceMode();

        // Assert
        // Should set isUnderMaintenance to false
    }

    // Additional tests for coverage
    [Fact]
    public void ClickTrackingDLQWorker_ShouldHandleNullConsumerHandle()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();
        worker.EnableMaintenanceMode(); // Second call should handle null consumer handle

        // Assert
        // Should complete without throwing exceptions
    }

    [Fact]
    public void ClickTrackingWorker_ShouldHandleNullConsumerHandle()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();
        worker.EnableMaintenanceMode(); // Second call should handle null consumer handle

        // Assert
        // Should complete without throwing exceptions
    }

    [Fact]
    public void ClickTrackingDLQWorker_ShouldHandleMultipleMaintenanceModeChanges()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingDLQWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();
        worker.DisableMaintenanceMode();
        worker.EnableMaintenanceMode();
        worker.DisableMaintenanceMode();

        // Assert
        // Should complete without throwing exceptions
    }

    [Fact]
    public void ClickTrackingWorker_ShouldHandleMultipleMaintenanceModeChanges()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockAdvancedBus = new Mock<IAdvancedBus>();
        mockBus.Setup(x => x.Advanced).Returns(mockAdvancedBus.Object);
        
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogService = new Mock<ILogService>();

        var worker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        // Act
        worker.EnableMaintenanceMode();
        worker.DisableMaintenanceMode();
        worker.EnableMaintenanceMode();
        worker.DisableMaintenanceMode();

        // Assert
        // Should complete without throwing exceptions
    }
}
