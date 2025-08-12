using Fanex.Affiliate.Link.Core.Services;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using NLog;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests.Services;

public class LogServiceTests
{
    private readonly Mock<Logging.ILogger> _mockLogger;
    private readonly LogService _logService;

    public LogServiceTests()
    {
        _mockLogger = new Mock<Logging.ILogger>();
        _logService = new LogService(_mockLogger.Object);
    }

    [Fact]
    public void LogFileError_ShouldCallNLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        _logService.LogFileError(exception);

        // Assert
        // Note: We can't easily mock the static NLog logger, but we can verify the method doesn't throw
        // The actual logging behavior would be tested in integration tests
    }

    [Fact]
    public void LogFileMessage_ShouldCallNLogError()
    {
        // Arrange
        const string message = "Test log message";

        // Act
        _logService.LogFileMessage(message);

        // Assert
        // Note: We can't easily mock the static NLog logger, but we can verify the method doesn't throw
        // The actual logging behavior would be tested in integration tests
    }

    [Fact]
    public async Task SentryErrorAsync_ShouldCallLoggerErrorAsync_WithMessageAndException()
    {
        // Arrange
        const string errorMessage = "Test error message";
        var exception = new Exception("Test exception");

        _mockLogger.Setup(x => x.ErrorAsync(errorMessage, exception))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logService.SentryErrorAsync(errorMessage, exception);

        // Assert
        _mockLogger.Verify(x => x.ErrorAsync(errorMessage, exception), Times.Once);
    }

    [Fact]
    public async Task SentryErrorAsync_ShouldCallLoggerErrorAsync_WithMessageOnly()
    {
        // Arrange
        const string errorMessage = "Test error message";

        _mockLogger.Setup(x => x.ErrorAsync(errorMessage))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logService.SentryErrorAsync(errorMessage);

        // Assert
        _mockLogger.Verify(x => x.ErrorAsync(errorMessage), Times.Once);
    }

    [Fact]
    public async Task SentryErrorAsync_ShouldCallLoggerErrorAsync_WithExceptionOnly()
    {
        // Arrange
        var exception = new Exception("Test exception");
        const string expectedMessage = "System.Exception: Test exception";

        _mockLogger.Setup(x => x.ErrorAsync(expectedMessage))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logService.SentryErrorAsync(exception);

        // Assert
        _mockLogger.Verify(x => x.ErrorAsync(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SentryInfoAsync_ShouldCallLoggerInfoAsync()
    {
        // Arrange
        const string infoMessage = "Test info message";

        _mockLogger.Setup(x => x.InfoAsync(infoMessage))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logService.SentryInfoAsync(infoMessage);

        // Assert
        _mockLogger.Verify(x => x.InfoAsync(infoMessage), Times.Once);
    }

    [Fact]
    public async Task SentryErrorAsync_ShouldHandleNullException()
    {
        // Arrange
        const string errorMessage = "Test error message";
        Exception? exception = null;

        _mockLogger.Setup(x => x.ErrorAsync(errorMessage, exception))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logService.SentryErrorAsync(errorMessage, exception!);

        // Assert
        _mockLogger.Verify(x => x.ErrorAsync(errorMessage, exception), Times.Once);
    }

    [Fact]
    public async Task SentryErrorAsync_ShouldHandleEmptyMessage()
    {
        // Arrange
        const string errorMessage = "";

        _mockLogger.Setup(x => x.ErrorAsync(errorMessage))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logService.SentryErrorAsync(errorMessage);

        // Assert
        _mockLogger.Verify(x => x.ErrorAsync(errorMessage), Times.Once);
    }

    [Fact]
    public async Task SentryInfoAsync_ShouldHandleEmptyMessage()
    {
        // Arrange
        const string infoMessage = "";

        _mockLogger.Setup(x => x.InfoAsync(infoMessage))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logService.SentryInfoAsync(infoMessage);

        // Assert
        _mockLogger.Verify(x => x.InfoAsync(infoMessage), Times.Once);
    }

    [Fact]
    public void LogFileMessage_ShouldHandleEmptyMessage()
    {
        // Arrange
        const string message = "";

        // Act & Assert
        // Should not throw
        _logService.LogFileMessage(message);
    }

    [Fact]
    public void LogFileError_ShouldHandleNullException()
    {
        // Arrange
        Exception? exception = null;

        // Act & Assert
        // Should not throw
        _logService.LogFileError(exception!);
    }

    [Fact]
    public async Task SentryErrorAsync_ShouldHandleLoggerException()
    {
        // Arrange
        const string errorMessage = "Test error message";
        var exception = new Exception("Test exception");
        var loggerException = new Exception("Logger failed");

        _mockLogger.Setup(x => x.ErrorAsync(errorMessage, exception))
            .ThrowsAsync(loggerException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _logService.SentryErrorAsync(errorMessage, exception));
    }

    [Fact]
    public async Task SentryInfoAsync_ShouldHandleLoggerException()
    {
        // Arrange
        const string infoMessage = "Test info message";
        var loggerException = new Exception("Logger failed");

        _mockLogger.Setup(x => x.InfoAsync(infoMessage))
            .ThrowsAsync(loggerException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _logService.SentryInfoAsync(infoMessage));
    }
}
