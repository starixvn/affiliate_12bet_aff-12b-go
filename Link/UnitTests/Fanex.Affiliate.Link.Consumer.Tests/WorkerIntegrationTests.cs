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

public class WorkerIntegrationTests
{
    private readonly Mock<IBus> _mockBus;
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IAdvancedBus> _mockAdvancedBus;
    private readonly Mock<IRedisService> _mockRedisService;

    public WorkerIntegrationTests()
    {
        _mockBus = new Mock<IBus>();
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockLogService = new Mock<ILogService>();
        _mockAdvancedBus = new Mock<IAdvancedBus>();
        _mockRedisService = new Mock<IRedisService>();

        _mockBus.Setup(x => x.Advanced).Returns(_mockAdvancedBus.Object);
    }

    [Fact]
    public void Workers_ShouldShareSameConstants()
    {
        // Arrange
        var clickTrackingWorker = new ClickTrackingWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);

        var clickTrackingDLQWorker = new ClickTrackingDLQWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);

        var clickTrackingRedisStorageWorker = new ClickTrackingRedisStorageWorker(
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockRedisService.Object,
            _mockLogService.Object);

        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }

    [Fact]
    public void Workers_ShouldUseSameMaintenanceService()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var clickTrackingWorker = new ClickTrackingWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);

        var clickTrackingDLQWorker = new ClickTrackingDLQWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);

        // Act
        clickTrackingWorker.CheckMaintenanceMode();
        clickTrackingDLQWorker.CheckMaintenanceMode();

        // Assert
        _mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Exactly(2));
    }

    [Fact]
    public void Workers_ShouldUseSameLogService()
    {
        // Arrange
        var clickTrackingWorker = new ClickTrackingWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);

        var clickTrackingDLQWorker = new ClickTrackingDLQWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);

        // Act
        clickTrackingWorker.EnableMaintenanceMode();
        clickTrackingDLQWorker.EnableMaintenanceMode();

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Exactly(2));
    }
}
