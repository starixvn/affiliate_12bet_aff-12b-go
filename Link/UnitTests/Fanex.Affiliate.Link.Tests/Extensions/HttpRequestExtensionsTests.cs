using Fanex.Affiliate.Link.Extensions;
using Microsoft.AspNetCore.Http;
using System.Net;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Extensions;

public class HttpRequestExtensionsTests
{
    [Fact]
    public void GetClientIP_ShouldReturnXActualIP_WhenPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Actual-IP"] = "192.168.1.100";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void GetClientIP_ShouldReturnXForwardedFor_WhenXActualIPNotPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = "192.168.1.200";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.200");
    }

    [Fact]
    public void GetClientIP_ShouldReturnRemoteIPAddress_WhenHeadersNotPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("127.0.0.1");
    }

    [Fact]
    public void GetClientIP_ShouldReturnLocalhost_WhenRemoteIPIsIPv6Localhost()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("127.0.0.1");
    }

    [Fact]
    public void GetClientIP_ShouldReturnFirstValidIP_WhenMultipleIPsInHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = "192.168.1.100, 10.0.0.1, invalid-ip";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void GetClientIP_ShouldReturnNull_WhenNoValidIPs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = "invalid-ip, another-invalid";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetClientIP_ShouldHandleEmptyHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = "";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetClientIP_ShouldHandleWhitespaceHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = "   ";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetClientIP_ShouldPrioritizeXActualIP_OverXForwardedFor()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Actual-IP"] = "192.168.1.100";
        request.Headers["X-Forwarded-For"] = "192.168.1.200";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }
}
