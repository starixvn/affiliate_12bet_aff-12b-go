using Fanex.Affiliate.Link.Constants;
using Fanex.Affiliate.Link.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Extensions;

public class HttpRequestExtensionsTests
{
    [Fact]
    public void GetClientIP_ShouldReturnXActualIP_WhenHeaderExists()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns("192.168.1.100");
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns("10.0.0.1");
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(new Mock<HttpContext>().Object);
        mockRequest.Setup(x => x.HttpContext.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void GetClientIP_ShouldReturnXForwardedFor_WhenXActualIPNotExists()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns((string)null);
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns("10.0.0.1");
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(new Mock<HttpContext>().Object);
        mockRequest.Setup(x => x.HttpContext.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().Be("10.0.0.1");
    }

    [Fact]
    public void GetClientIP_ShouldReturnRemoteIPAddress_WhenHeadersNotExist()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns((string)null);
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns((string)null);
        mockConnection.Setup(x => x.RemoteIpAddress).Returns(IPAddress.Parse("172.16.0.1"));
        
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().Be("172.16.0.1");
    }

    [Fact]
    public void GetClientIP_ShouldReturnLocalhost_WhenRemoteIPIsIPv6Localhost()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns((string)null);
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns((string)null);
        mockConnection.Setup(x => x.RemoteIpAddress).Returns(IPAddress.IPv6Loopback);
        
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().Be("127.0.0.1");
    }

    [Fact]
    public void GetClientIP_ShouldReturnFirstValidIP_WhenMultipleIPsInHeader()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns((string)null);
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns("192.168.1.100, 10.0.0.1, 172.16.0.1");
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(new Mock<HttpContext>().Object);
        mockRequest.Setup(x => x.HttpContext.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void GetClientIP_ShouldReturnNull_WhenNoValidIPsFound()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns((string)null);
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns("invalid-ip, another-invalid");
        mockConnection.Setup(x => x.RemoteIpAddress).Returns((IPAddress)null);
        
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetClientIP_ShouldHandleEmptyHeaders()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns("");
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns("");
        mockConnection.Setup(x => x.RemoteIpAddress).Returns(IPAddress.Parse("192.168.1.1"));
        
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.1");
    }

    [Fact]
    public void GetClientIP_ShouldHandleWhitespaceInHeaders()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConnection = new Mock<ConnectionInfo>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHeaders.Setup(x => x[HeaderKey.X_ACTUAL_IP]).Returns("   ");
        mockHeaders.Setup(x => x[HeaderKey.X_FORWARDED_FOR]).Returns("  192.168.1.100  ");
        mockConnection.Setup(x => x.RemoteIpAddress).Returns(IPAddress.Parse("172.16.0.1"));
        
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);
        mockRequest.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);

        // Act
        var result = mockRequest.Object.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }
}
