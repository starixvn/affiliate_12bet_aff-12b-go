using EasyNetQ;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class ClickTrackingWorkerSimpleTests
{
    private readonly Mock<IBus> _mockBus;
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly ClickTrackingWorker _worker;

    public ClickTrackingWorkerSimpleTests()
    {
        _mockBus = new Mock<IBus>();
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockLogService = new Mock<ILogService>();

        _worker = new ClickTrackingWorker(
            _mockBus.Object,
            _mockMaintenanceService.Object,
            _mockDatabaseService.Object,
            _mockLogService.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Act & Assert
        _worker.Should().NotBeNull();
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
    public void EnableMaintenanceMode_ShouldNotLogMessage_WhenAlreadyInMaintenanceMode()
    {
        // Arrange
        _worker.EnableMaintenanceMode(); // First call
        _mockLogService.Reset();

        // Act
        _worker.EnableMaintenanceMode(); // Second call

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
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
    public void CheckMaintenanceMode_ShouldCallMaintenanceService_WhenCalled()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Act
        _worker.CheckMaintenanceMode();

        // Assert
        _mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteSuccessfully()
    {
        // Act & Assert - Should not throw
        await _worker.StopAsync(CancellationToken.None);
    }
}
