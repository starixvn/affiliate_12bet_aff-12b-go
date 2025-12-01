using Fanex.Affiliate.Link.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Middleware;

public class AffiliateErrorMiddlewareTests
{
    private static async Task<string> InvokeAndReadAsync(string path, int initialStatus)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        var next = new RequestDelegate(ctx =>
        {
            ctx.Response.StatusCode = initialStatus;
            return Task.CompletedTask;
        });

        var logger = new Mock<ILogger<AffiliateErrorMiddleware>>();
        var middleware = new AffiliateErrorMiddleware(next, logger.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShouldRenderErrorPage_When404AndAffiliatePattern()
    {
        var html = await InvokeAndReadAsync("/home/123/en/index.html", 404);

        html.Should().Contain("This link is in an incorrect format. Please contact administrator!");
        html.Should().Contain("Affiliate link format is correct");
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotHandle_WhenNot404()
    {
        var html = await InvokeAndReadAsync("/home/123/en/index.html", 200);
        html.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotHandle_WhenPathEmpty()
    {
        var html = await InvokeAndReadAsync("/", 404);
        html.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldDetectInvalidLanguage()
    {
        var html = await InvokeAndReadAsync("/home/123/xx/index.html", 404);
        // HTML is encoded, so compare encoded content
        html.Should().Contain("Invalid language &#39;xx&#39;.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSupportFiveSegmentPattern()
    {
        var html = await InvokeAndReadAsync("/sports/cricket/123/en/mobile/index.html", 404);
        html.Should().Contain("Affiliate link format is correct, but the combination &#39;sports/cricket&#39;");
    }

    [Theory]
    [InlineData("/home/abc/en/index.html")]
    [InlineData("/home/123/xyz/index.html")]
    [InlineData("/123abc/123/en/index.html")]
    [InlineData("/home/index.html")]
    [InlineData("/index.html")]
    public async Task InvokeAsync_ShouldDetectVariousErrors(string path)
    {
        var html = await InvokeAndReadAsync(path, 404);
        html.Should().Contain("This link is in an incorrect format. Please contact administrator!");
        html.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("/123/sports/cricket/en/index.html")] // Legacy 4-segment pattern
    [InlineData("/sports/cricket/123/en/mobile/index.html")] // 5-segment pattern
    [InlineData("/123/home/en/index.html")] // Legacy 3-segment pattern  
    public async Task InvokeAsync_ShouldHandleDifferentPatterns(string path)
    {
        var html = await InvokeAndReadAsync(path, 404);
        html.Should().Contain("This link is in an incorrect format. Please contact administrator!");
        html.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogWarning_WhenHandlingError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/home/123/en/index.html";
        context.Response.Body = new MemoryStream();

        var next = new RequestDelegate(ctx =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        var logger = new Mock<ILogger<AffiliateErrorMiddleware>>();
        var middleware = new AffiliateErrorMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Affiliate URL validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetContextItems_WhenHandlingError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/home/123/en/index.html";
        context.Response.Body = new MemoryStream();

        var next = new RequestDelegate(ctx =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        var logger = new Mock<ILogger<AffiliateErrorMiddleware>>();
        var middleware = new AffiliateErrorMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CustomErrorMessage");
        context.Items.Should().ContainKey("OriginalPath");
        context.Items["OriginalPath"].Should().Be("/home/123/en/index.html");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrectContentType_WhenRenderingErrorPage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/home/123/en/index.html";
        context.Response.Body = new MemoryStream();

        var next = new RequestDelegate(ctx =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        var logger = new Mock<ILogger<AffiliateErrorMiddleware>>();
        var middleware = new AffiliateErrorMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("text/html; charset=utf-8");
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotHandle_WhenPathDoesNotEndWithIndexHtml()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/some/path/without/indexhtml";
        context.Response.Body = new MemoryStream();

        var next = new RequestDelegate(ctx =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        var logger = new Mock<ILogger<AffiliateErrorMiddleware>>();
        var middleware = new AffiliateErrorMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        var html = await reader.ReadToEndAsync();
        html.Should().BeEmpty();
    }

    [Theory]
    [InlineData("/test/path", 404, false)] // No index.html
    [InlineData("/test/path/index.html", 200, false)] // Not 404
    [InlineData("/test/path/index.html", 404, true)] // Should handle
    [InlineData("", 404, false)] // Empty path
    public async Task ShouldHandleError_ShouldReturnCorrectResult(string path, int statusCode, bool shouldHandle)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        var next = new RequestDelegate(ctx =>
        {
            ctx.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        });

        var logger = new Mock<ILogger<AffiliateErrorMiddleware>>();
        var middleware = new AffiliateErrorMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        var html = await reader.ReadToEndAsync();

        if (shouldHandle)
        {
            html.Should().Contain("This link is in an incorrect format");
        }
        else
        {
            html.Should().BeEmpty();
        }
    }
}

// (Removed duplicate test class with misplaced using directives)
