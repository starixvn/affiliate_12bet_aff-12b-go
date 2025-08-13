using System;
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

public class ClickTrackingRedisStorageWorkerEnhancedTests
{
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<IRedisService> _mockRedisService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly ClickTrackingRedisStorageWorker _worker;

    public ClickTrackingRedisStorageWorkerEnhancedTests()
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
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Act & Assert
        _worker.Should().NotBeNull();
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
    public async Task StartConsumer_ShouldNotRun_WhenUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Act
        var startConsumerMethod = typeof(ClickTrackingRedisStorageWorker).GetMethod("StartConsumer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task)startConsumerMethod!.Invoke(_worker, null)!;
        await task;

        // Assert
        _mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(It.IsAny<string>()), Times.Never);
        _mockDatabaseService.Verify(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()), Times.Never);
    }

    [Fact]
    public async Task StartConsumer_ShouldProcessMessages_WhenAvailable()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var clickEvent = CreateValidClickTrackingEvent();
        _mockRedisService.SetupSequence(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync(clickEvent)
            .ReturnsAsync((ClickTrackingEvent)null);

        _mockDatabaseService.Setup(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var startConsumerMethod = typeof(ClickTrackingRedisStorageWorker).GetMethod("StartConsumer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task)startConsumerMethod!.Invoke(_worker, null)!;
        await task;

        // Assert
        _mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName), Times.AtLeast(2));
        _mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent), Times.Once);
    }

    [Fact]
    public async Task StartConsumer_ShouldHandleException_AndLogError()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var exception = new Exception("Test exception");
        _mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ThrowsAsync(exception);

        // Act
        var startConsumerMethod = typeof(ClickTrackingRedisStorageWorker).GetMethod("StartConsumer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task)startConsumerMethod!.Invoke(_worker, null)!;
        await task;

        // Assert
        _mockLogService.Verify(x => x.SentryErrorAsync(exception), Times.Once);
    }

    [Fact]
    public async Task StartConsumer_ShouldProcessMultipleMessages()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var clickEvent1 = CreateValidClickTrackingEvent(1, "TEST1");
        var clickEvent2 = CreateValidClickTrackingEvent(2, "TEST2");

        _mockRedisService.SetupSequence(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync(clickEvent1)
            .ReturnsAsync(clickEvent2)
            .ReturnsAsync((ClickTrackingEvent)null);

        _mockDatabaseService.Setup(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var startConsumerMethod = typeof(ClickTrackingRedisStorageWorker).GetMethod("StartConsumer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task)startConsumerMethod!.Invoke(_worker, null)!;
        await task;

        // Assert
        _mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent1), Times.Once);
        _mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent2), Times.Once);
        _mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName), Times.Exactly(3));
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteSuccessfully()
    {
        // Act & Assert - Should not throw
        await _worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }

    private static ClickTrackingEvent CreateValidClickTrackingEvent(long clickId = 123, string affCode = "TEST")
    {
        return new ClickTrackingEvent
        {
            ClickId = clickId,
            AffCode = affCode,
            AffDomain = "test.com",
            PageCode = "PAGE1",
            LanguageCode = "EN",
            ChannelCode = "WEB",
            IP = "127.0.0.1",
            Browser = "Chrome",
            UrlReferrer = "http://test.com",
            DomainReferrer = "test.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "100.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0",
            Country = "US",
            RedirectLink = "http://redirect.com",
            AffMediaLink = "http://media.com"
        };
    }
}
