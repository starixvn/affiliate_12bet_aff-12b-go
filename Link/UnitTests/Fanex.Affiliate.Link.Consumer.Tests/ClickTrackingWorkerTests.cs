using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Consumer;
using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class ClickTrackingWorkerTests
{
    private readonly Mock<IBus> _mockBus;
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IAdvancedBus> _mockAdvancedBus;
    private readonly Mock<IDisposable> _mockConsumerHandle;
    private readonly ClickTrackingWorker _worker;

    public ClickTrackingWorkerTests()
    {
        _mockBus = new Mock<IBus>();
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockLogService = new Mock<ILogService>();
        _mockAdvancedBus = new Mock<IAdvancedBus>();
        _mockConsumerHandle = new Mock<IDisposable>();

        _mockBus.Setup(x => x.Advanced).Returns(_mockAdvancedBus.Object);

        _worker = new ClickTrackingWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);
    }

    [Fact]
    public void EnableMaintenanceMode_ShouldLogMessage_WhenCalled()
    {
        // Act
        _worker.EnableMaintenanceMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public void DisableMaintenanceMode_ShouldLogMessage_WhenUnderMaintenance()
    {
        // Arrange
        _worker.EnableMaintenanceMode();

        // Act
        _worker.DisableMaintenanceMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void CheckMaintenanceMode_ShouldEnableMaintenanceMode_WhenUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Act
        _worker.CheckMaintenanceMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
    }

    [Fact]
    public void CheckMaintenanceMode_ShouldDisableMaintenanceMode_WhenNotUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);
        
        // First enable maintenance mode to set the internal state
        _worker.EnableMaintenanceMode();

        // Act
        _worker.CheckMaintenanceMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode disabled – message processing resumed."), Times.Once);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }
}
