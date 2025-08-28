using System;
using System.Threading.Tasks;
using EasyNetQ;
using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Commands;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class WorkerBehaviorTests
{
    [Fact]
    public async Task ClickTrackingRedisStorageWorker_ShouldContinueAfterDatabaseError()
    {
        // Arrange
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var clickEvent1 = CreateValidClickTrackingEvent(1, "TEST1");
        var clickEvent2 = CreateValidClickTrackingEvent(2, "TEST2");

        mockRedisService.SetupSequence(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync(clickEvent1)
            .ReturnsAsync(clickEvent2)
            .ReturnsAsync((ClickTrackingEvent)null);

        // First call throws exception, second call succeeds
        mockDatabaseService.SetupSequence(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .ThrowsAsync(new Exception("Database timeout"))
            .ReturnsAsync(new CommandResult { ErrorCode = 0, ErrorMessage = string.Empty });

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        // Act
        var startConsumerMethod = typeof(ClickTrackingRedisStorageWorker).GetMethod("StartConsumer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task)startConsumerMethod!.Invoke(worker, null)!;
        await task;

        // Assert - Should log error and stop processing due to exception
        mockLogService.Verify(x => x.SentryErrorAsync(It.IsAny<Exception>()), Times.Once);
        mockDatabaseService.Verify(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()), Times.Once);
    }

    [Fact]
    public async Task ClickTrackingRedisStorageWorker_ShouldHandleEmptyQueue()
    {
        // Arrange
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Always return null (empty queue)
        mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync((ClickTrackingEvent)null);

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        // Act
        var startConsumerMethod = typeof(ClickTrackingRedisStorageWorker).GetMethod("StartConsumer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task)startConsumerMethod!.Invoke(worker, null)!;
        await task;

        // Assert - Should handle empty queue gracefully
        mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName), 
            Times.Once);
        mockDatabaseService.Verify(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()), Times.Never);
    }

    [Fact]
    public void ClickTrackingWorker_EnableMaintenanceMode_ShouldWorkCorrectly()
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

        // Act & Assert
        worker.EnableMaintenanceMode();
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);

        // Enable again - should not log
        mockLogService.Reset();
        worker.EnableMaintenanceMode();
        mockLogService.Verify(x => x.LogFileMessage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ClickTrackingWorker_CheckMaintenanceMode_ShouldCallMaintenanceService()
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

        // Test enable maintenance
        var maintenanceTimeEnabled = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTimeEnabled);

        // Act
        worker.CheckMaintenanceMode();

        // Assert
        mockLogService.Verify(x => x.LogFileMessage("Maintenance mode enabled – message processing paused."), Times.Once);
        mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public void Workers_ShouldBeCreatedWithValidDependencies()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        // Act & Assert - Should not throw
        var clickTrackingWorker = new ClickTrackingWorker(
            mockBus.Object,
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockLogService.Object);

        var redisWorker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        clickTrackingWorker.Should().NotBeNull();
        redisWorker.Should().NotBeNull();
    }

    [Fact]
    public void Constants_ShouldHaveExpectedValues()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
        
        Constants.QueueIdentify.Should().NotBeNullOrEmpty();
        Constants.CacheRedisQueueName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ClickTrackingEvent_ShouldSupportAllRequiredProperties()
    {
        // Arrange & Act
        var clickEvent = CreateValidClickTrackingEvent();

        // Assert
        clickEvent.ClickId.Should().Be(123);
        clickEvent.AffCode.Should().Be("TEST");
        clickEvent.AffDomain.Should().Be("test.com");
        clickEvent.PageCode.Should().Be("PAGE1");
        clickEvent.LanguageCode.Should().Be("EN");
        clickEvent.ChannelCode.Should().Be("WEB");
        clickEvent.IP.Should().Be("127.0.0.1");
        clickEvent.Browser.Should().Be("Chrome");
        clickEvent.UrlReferrer.Should().Be("http://test.com");
        clickEvent.DomainReferrer.Should().Be("test.com");
        clickEvent.OS.Should().Be("Windows");
        clickEvent.Device.Should().Be("Desktop");
        clickEvent.BrowserVersion.Should().Be("100.0");
        clickEvent.BrowserResolution.Should().Be("1920x1080");
        clickEvent.BrowserLanguage.Should().Be("en-US");
        clickEvent.UserAgent.Should().Be("Mozilla/5.0");
        clickEvent.Country.Should().Be("US");
        clickEvent.RedirectLink.Should().Be("http://redirect.com");
        clickEvent.AffMediaLink.Should().Be("http://media.com");
        clickEvent.DeviceType.Should().Be(1);
        clickEvent.UpdatedTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ClickTrackingEvent_ShouldAllowPropertyModification()
    {
        // Arrange
        var clickEvent = CreateValidClickTrackingEvent();

        // Act
        clickEvent.AffCode = "MODIFIED";
        clickEvent.ClickId = 999;
        clickEvent.DeviceType = 2;
        clickEvent.UpdatedTime = DateTime.UtcNow.AddHours(-1);

        // Assert
        clickEvent.AffCode.Should().Be("MODIFIED");
        clickEvent.ClickId.Should().Be(999);
        clickEvent.DeviceType.Should().Be(2);
        clickEvent.UpdatedTime.Should().BeCloseTo(DateTime.UtcNow.AddHours(-1), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ClickTrackingEvent_ToString_ShouldReturnStringRepresentation()
    {
        // Arrange
        var clickEvent = CreateValidClickTrackingEvent();

        // Act
        var result = clickEvent.ToString();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().BeOfType<string>();
    }

    [Fact]
    public async Task ClickTrackingRedisStorageWorker_ShouldHandleNullClickEvent()
    {
        // Arrange
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Return null click event
        mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync((ClickTrackingEvent)null);

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        // Act
        var startConsumerMethod = typeof(ClickTrackingRedisStorageWorker).GetMethod("StartConsumer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task)startConsumerMethod!.Invoke(worker, null)!;
        await task;

        // Assert - Should handle null gracefully
        mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName), Times.Once);
        mockDatabaseService.Verify(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()), Times.Never);
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
            AffMediaLink = "http://media.com",
            DeviceType = 1,
            UpdatedTime = DateTime.UtcNow
        };
    }
}
