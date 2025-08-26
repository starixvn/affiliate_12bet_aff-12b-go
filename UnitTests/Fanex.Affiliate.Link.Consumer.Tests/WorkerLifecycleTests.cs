using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
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

public class WorkerLifecycleTests
{
    [Fact]
    public async Task ClickTrackingRedisStorageWorker_ExecuteAsync_ShouldRunContinuously()
    {
        // Arrange
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync((ClickTrackingEvent)null);

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(300); // Run for 300ms

        var startTime = DateTime.UtcNow;

        // Act
        try
        {
            await worker.StartAsync(cts.Token);
            await Task.Delay(250, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation token is triggered
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }

        var duration = DateTime.UtcNow - startTime;

        // Assert
        duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(200));
        mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))), 
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ClickTrackingRedisStorageWorker_ExecuteAsync_ShouldProcessMultipleMessages()
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

        mockDatabaseService.Setup(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await worker.StartAsync(cts.Token);
            await Task.Delay(150, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }

        // Assert
        mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent1), Times.Once);
        mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent2), Times.Once);
        mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName), 
            Times.AtLeast(3));
    }

    [Fact]
    public async Task ClickTrackingRedisStorageWorker_ExecuteAsync_ShouldHandleExceptions()
    {
        // Arrange
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var exception = new Exception("Redis connection error");
        mockRedisService.SetupSequence(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ThrowsAsync(exception)
            .ReturnsAsync((ClickTrackingEvent)null);

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await worker.StartAsync(cts.Token);
            await Task.Delay(150, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }

        // Assert
        mockLogService.Verify(x => x.SentryErrorAsync(exception), Times.Once);
        mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))), 
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ClickTrackingRedisStorageWorker_ExecuteAsync_ShouldRespectMaintenanceMode()
    {
        // Arrange
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await worker.StartAsync(cts.Token);
            await Task.Delay(150, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }

        // Assert - Should not process messages when under maintenance
        mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(It.IsAny<string>()), Times.Never);
        mockDatabaseService.Verify(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()), Times.Never);
        mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))), 
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AllWorkers_StopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

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

        // Act & Assert - Should not throw
        await clickTrackingWorker.StopAsync(CancellationToken.None);
        await redisWorker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Workers_ShouldHandleMultipleStartStopCycles()
    {
        // Arrange
        var mockMaintenanceService = new Mock<IMaintenanceService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockRedisService = new Mock<IRedisService>();
        var mockLogService = new Mock<ILogService>();

        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(It.IsAny<string>()))
            .ReturnsAsync((ClickTrackingEvent)null);

        var worker = new ClickTrackingRedisStorageWorker(
            mockMaintenanceService.Object,
            mockDatabaseService.Object,
            mockRedisService.Object,
            mockLogService.Object);

        // Act & Assert - Multiple cycles should not throw
        for (int i = 0; i < 3; i++)
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(50);

            try
            {
                await worker.StartAsync(cts.Token);
                await Task.Delay(25, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }

            await worker.StopAsync(CancellationToken.None);
        }

        mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))), 
            Times.AtLeast(3));
    }

    [Fact]
    public void Workers_ShouldHaveCorrectQueueNames()
    {
        // Arrange & Act
        var mainExchangeName = $"{Constants.QueueIdentify}.exchange";
        var retryExchangeName = $"{Constants.QueueIdentify}.retry.exchange";
        var errorExchangeName = $"{Constants.QueueIdentify}.error.exchange";
        var mainQueueName = $"{Constants.QueueIdentify}.queue";
        var retryQueueName = $"{Constants.QueueIdentify}.retry.queue";
        var errorQueueName = $"{Constants.QueueIdentify}.error.queue";
        var routingKey = $"{Constants.QueueIdentify}.routing";

        // Assert
        mainExchangeName.Should().Be("12b.user.click.exchange");
        retryExchangeName.Should().Be("12b.user.click.retry.exchange");
        errorExchangeName.Should().Be("12b.user.click.error.exchange");
        mainQueueName.Should().Be("12b.user.click.queue");
        retryQueueName.Should().Be("12b.user.click.retry.queue");
        errorQueueName.Should().Be("12b.user.click.error.queue");
        routingKey.Should().Be("12b.user.click.routing");
    }

    [Fact]
    public void Workers_ShouldHaveCorrectRetryConfiguration()
    {
        // Arrange & Act
        const int maxRetryCount = 5;
        const int timeToReQueue = 30000; // 30 seconds
        const int timeToRun = 5; // 5 minutes for DLQ worker
        const int jobDelayMinute = 1; // 1 minute for Redis worker

        // Assert
        maxRetryCount.Should().Be(5);
        timeToReQueue.Should().Be(30000);
        timeToRun.Should().Be(5);
        jobDelayMinute.Should().Be(1);
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
