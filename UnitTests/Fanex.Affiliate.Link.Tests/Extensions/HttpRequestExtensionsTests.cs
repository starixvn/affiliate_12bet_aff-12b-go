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

    [Theory]
    [InlineData("192.168.1.1", "192.168.1.1")]
    [InlineData("10.0.0.1", "10.0.0.1")]
    [InlineData("172.16.0.1", "172.16.0.1")]
    [InlineData("203.0.113.1", "203.0.113.1")]
    public void GetClientIP_ShouldReturnValidIPv4Addresses(string inputIP, string expectedIP)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Actual-IP"] = inputIP;

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be(expectedIP);
    }

    [Theory]
    [InlineData("192.168.1.1, 10.0.0.1, 172.16.0.1", "192.168.1.1")]
    [InlineData("invalid, 192.168.1.1, another-invalid", "192.168.1.1")]
    [InlineData("   192.168.1.1   , 10.0.0.1", "192.168.1.1")]
    public void GetClientIP_ShouldReturnFirstValidIP_FromCommaSeparatedList(string headerValue, string expectedIP)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = headerValue;

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be(expectedIP);
    }

    [Theory]
    [InlineData("256.256.256.256")]
    [InlineData("192.168.1")]
    [InlineData("192.168.1.1.1")]
    [InlineData("abc.def.ghi.jkl")]
    [InlineData("192.168.-1.1")]
    public void GetClientIP_ShouldIgnoreInvalidIPFormats(string invalidIP)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = invalidIP;

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetClientIP_ShouldHandleMixedValidAndInvalidIPs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = "invalid-ip, 256.256.256.256, 192.168.1.100, another-invalid";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void GetClientIP_ShouldHandleIPv6Localhost_WhenNoHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        context.Connection.RemoteIpAddress = System.Net.IPAddress.IPv6Loopback; // ::1

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("127.0.0.1");
    }

    [Fact]
    public void GetClientIP_ShouldHandleIPv4Localhost_WhenNoHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback; // 127.0.0.1

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("127.0.0.1");
    }

    [Fact]
    public void GetClientIP_ShouldHandleNullRemoteIpAddress()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        context.Connection.RemoteIpAddress = null;

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("0.0.0.0", "0.0.0.0")]
    [InlineData("255.255.255.255", "255.255.255.255")]
    [InlineData("127.0.0.1", "127.0.0.1")]
    public void GetClientIP_ShouldHandleEdgeCaseIPAddresses(string inputIP, string expectedIP)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Actual-IP"] = inputIP;

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be(expectedIP);
    }

    [Fact]
    public void GetClientIP_ShouldTrimWhitespaceFromIPs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = "  192.168.1.100  ,   10.0.0.1   ";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void GetClientIP_ShouldHandleEmptyCommaSeparatedValues()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers["X-Forwarded-For"] = ", , 192.168.1.100, ,";

        // Act
        var result = request.GetClientIP();

        // Assert
        result.Should().Be("192.168.1.100");
    }
}
