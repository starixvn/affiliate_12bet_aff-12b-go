using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Commands;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class ClickTrackingRedisStorageWorkerIntegrationTests
{
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<IRedisService> _mockRedisService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly ClickTrackingRedisStorageWorker _worker;

    public ClickTrackingRedisStorageWorkerIntegrationTests()
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
    public async Task ExecuteAsync_ShouldProcessMessages_FromRedisQueue()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var clickEvent = CreateValidClickTrackingEvent();
        _mockRedisService.SetupSequence(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync(clickEvent)
            .ReturnsAsync((ClickTrackingEvent)null);

        _mockDatabaseService.Setup(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new CommandResult { ErrorCode = 0, ErrorMessage = string.Empty });

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(100, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await _worker.StopAsync(CancellationToken.None);
        }

        // Assert
        _mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName),
            Times.AtLeastOnce);
        _mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogWorkerStatus_Periodically()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        _mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(It.IsAny<string>()))
            .ReturnsAsync((ClickTrackingEvent)null);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(100, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await _worker.StopAsync(CancellationToken.None);
        }

        // Assert
        _mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessMultipleMessages_InSequence()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var clickEvent1 = CreateValidClickTrackingEvent(1, "TEST1");
        var clickEvent2 = CreateValidClickTrackingEvent(2, "TEST2");
        var clickEvent3 = CreateValidClickTrackingEvent(3, "TEST3");

        _mockRedisService.SetupSequence(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync(clickEvent1)
            .ReturnsAsync(clickEvent2)
            .ReturnsAsync(clickEvent3)
            .ReturnsAsync((ClickTrackingEvent)null);

        _mockDatabaseService.Setup(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new CommandResult { ErrorCode = 0, ErrorMessage = string.Empty });

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(300);

        // Act
        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(200, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await _worker.StopAsync(CancellationToken.None);
        }

        // Assert
        _mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent1), Times.Once);
        _mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent2), Times.Once);
        _mockDatabaseService.Verify(x => x.InsertClickInfo(clickEvent3), Times.Once);
        _mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName),
            Times.AtLeast(4));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleExceptions_AndContinueProcessing()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        var exception = new Exception("Redis connection error");
        _mockRedisService.SetupSequence(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ThrowsAsync(exception)
            .ReturnsAsync((ClickTrackingEvent)null);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(100, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await _worker.StopAsync(CancellationToken.None);
        }

        // Assert
        _mockLogService.Verify(x => x.SentryErrorAsync(exception), Times.Once);
        _mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectMaintenanceMode_WhenEnabled()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = true };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(100, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await _worker.StopAsync(CancellationToken.None);
        }

        // Assert - Should not process messages when under maintenance
        _mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(It.IsAny<string>()), Times.Never);
        _mockDatabaseService.Verify(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRunContinuously_UntilCancelled()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        _mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(It.IsAny<string>()))
            .ReturnsAsync((ClickTrackingEvent)null);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(400); // Run longer to test continuous operation

        var startTime = DateTime.UtcNow;

        // Act
        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(350, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await _worker.StopAsync(CancellationToken.None);
        }

        var duration = DateTime.UtcNow - startTime;

        // Assert
        duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(300));
        _mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessEmptyQueue_Gracefully()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        // Always return null (empty queue)
        _mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName))
            .ReturnsAsync((ClickTrackingEvent)null);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        // Act
        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(100, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            await _worker.StopAsync(CancellationToken.None);
        }

        // Assert - Should handle empty queue gracefully
        _mockRedisService.Verify(x => x.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName),
            Times.AtLeastOnce);
        _mockDatabaseService.Verify(x => x.InsertClickInfo(It.IsAny<ClickTrackingEvent>()), Times.Never);
        _mockLogService.Verify(x => x.LogFileMessage(
            It.Is<string>(s => s.Contains("ClickTrackingRedisStorageWorker running at:"))),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteSuccessfully_WhenCalled()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTime()).Returns(maintenanceTime);

        _mockRedisService.Setup(x => x.ListLeftPopAsync<ClickTrackingEvent>(It.IsAny<string>()))
            .ReturnsAsync((ClickTrackingEvent)null);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        try
        {
            await _worker.StartAsync(cts.Token);
            await Task.Delay(50, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Act & Assert - Should not throw
        await _worker.StopAsync(CancellationToken.None);
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
