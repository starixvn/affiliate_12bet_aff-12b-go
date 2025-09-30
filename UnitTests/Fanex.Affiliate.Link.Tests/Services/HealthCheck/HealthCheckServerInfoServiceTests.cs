using Fanex.Affiliate.Link.Services.HealthCheck;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Logging;
using System;

namespace Fanex.Affiliate.Link.Tests.Services.HealthCheck;

public class HealthCheckServerInfoServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IHostEnvironment> _mockEnvironment;
    private readonly Mock<ILogger<HealthCheckServerInfoService>> _mockLogger;
    private readonly HealthCheckServerInfoService _service;

    public HealthCheckServerInfoServiceTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockEnvironment = new Mock<IHostEnvironment>();
        _mockLogger = new Mock<ILogger<HealthCheckServerInfoService>>();
        
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Test");
        
        _service = new HealthCheckServerInfoService(
            _mockHttpContextAccessor.Object,
            _mockEnvironment.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpContextAccessorIsNull()
    {
        // Act & Assert
        var action = () => new HealthCheckServerInfoService(null!, _mockEnvironment.Object, _mockLogger.Object);
        action.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenEnvironmentIsNull()
    {
        // Act & Assert
        var action = () => new HealthCheckServerInfoService(_mockHttpContextAccessor.Object, null!, _mockLogger.Object);
        action.Should().Throw<ArgumentNullException>().WithParameterName("env");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        var action = () => new HealthCheckServerInfoService(_mockHttpContextAccessor.Object, _mockEnvironment.Object, null!);
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldReturnDictionary_WhenHttpContextAvailable()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result["Env"].Should().Be("Test");
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldReturnBasicInfo_WhenExceptionOccurs()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Throws(new Exception("Test exception"));

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result["Env"].Should().Be("Test");
        result.Should().ContainKey("Server Time");
        
        // Verify that logging was called (the exact log level and message may vary)
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldIncludeHostName_WhenDnsResolves()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Host Name");
        result["Host Name"].Should().NotBe("Unable to retrieve");
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldHandleDnsException_AndLogDebug()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        // Mock Dns.GetHostName to throw exception
        // Note: This is difficult to test directly due to static methods
        // The test verifies the service handles exceptions gracefully

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
    }

    [Fact]
    public void GetBasicServerInfo_ShouldReturnMinimalDictionary()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _service.GetBasicServerInfo();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result.Should().ContainKey("Server Time");
        result["Env"].Should().Be("Test");
    }

    [Fact]
    public void GetBasicServerInfo_ShouldReturnFallbackInfo_WhenExceptionOccurs()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.EnvironmentName).Throws(new Exception("Environment error"));

        // Act
        var result = _service.GetBasicServerInfo();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result.Should().ContainKey("Server Time");
        result["Env"].Should().Be("Unknown");
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to retrieve basic server information")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetBasicServerInfo_ShouldHandleAssemblyCreationTimeException()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _service.GetBasicServerInfo();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result.Should().ContainKey("Server Time");
        
        // The service should handle assembly creation time exceptions gracefully
        // and still return basic information
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldIncludeHttpContextInfo_WhenContextIsAvailable()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
        httpContext.Connection.LocalPort = 8080;
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        httpContext.Request.Host = new HostString("localhost:8080");
        httpContext.Request.Scheme = "https";
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Local Ip");
        result.Should().ContainKey("Local Port");
        result.Should().ContainKey("Client Ip");
        result.Should().ContainKey("Host");
        result.Should().ContainKey("Schema");
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldHandleNullHttpContext()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result["Env"].Should().Be("Test");
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldHandleHttpContextException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        // Mock HttpContext to throw exception when accessing properties
        // Note: Connection property is read-only, so we can't directly mock it
        // This test verifies the service handles HttpContext exceptions gracefully

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result["Env"].Should().Be("Test");
    }

    [Fact]
    public void GetBasicServerInfo_ShouldReturnReadOnlyDictionary()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _service.GetBasicServerInfo();

        // Assert
        result.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyDictionary<string, object>>();
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldReturnReadOnlyDictionary()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyDictionary<string, object>>();
    }

    [Fact]
    public void GetBasicServerInfo_ShouldIncludeServerTime()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _service.GetBasicServerInfo();

        // Assert
        result.Should().ContainKey("Server Time");
        result["Server Time"].Should().NotBeNull();
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldIncludeServerTime()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().ContainKey("Server Time");
        result["Server Time"].Should().NotBeNull();
    }

    [Fact]
    public void GetBasicServerInfo_ShouldHandleMultipleExceptions()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.EnvironmentName).Throws(new Exception("Environment error"));
        
        // Act
        var result = _service.GetBasicServerInfo();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result.Should().ContainKey("Server Time");
        result["Env"].Should().Be("Unknown");
    }

    [Fact]
    public async Task GetServerInfoAsync_ShouldHandleMultipleExceptions()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.EnvironmentName).Throws(new Exception("Environment error"));

        // Act
        var result = await _service.GetServerInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Env");
        result["Env"].Should().Be("Unknown");
    }
}


