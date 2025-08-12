using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

public class ClickTrackingRedisStorageWorkerTests
{
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<IRedisService> _mockRedisService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly ClickTrackingRedisStorageWorker _worker;

    public ClickTrackingRedisStorageWorkerTests()
    {
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockRedisService = new Mock<IRedisService>();
        _mockLogService = new Mock<ILogService>();

        _worker = new ClickTrackingRedisStorageWorker(
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockRedisService.Object,
            _mockLogService.Object);
    }

    [Fact]
    public void IsCurrentlyUnderMaintenance_ShouldReturnTrue_WhenUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Act
        var result = _worker.IsCurrentlyUnderMaintenance();

        // Assert
        result.Should().BeTrue();
        _mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public void IsCurrentlyUnderMaintenance_ShouldReturnFalse_WhenNotUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Act
        var result = _worker.IsCurrentlyUnderMaintenance();

        // Assert
        result.Should().BeFalse();
        _mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }
}

// Extension method to access private method for testing
public static class ClickTrackingRedisStorageWorkerExtensions
{
    public static bool IsCurrentlyUnderMaintenance(this ClickTrackingRedisStorageWorker worker)
    {
        var method = typeof(ClickTrackingRedisStorageWorker).GetMethod("IsCurrentlyUnderMaintenance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)method!.Invoke(worker, null)!;
    }
}
