using Fanex.Affiliate.Link.Services.Impl;
using Fanex.Affiliate.Link.Services.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Services;

public class HttpContextServiceTests
{
    private static IHttpContextService CreateService(DefaultHttpContext context)
    {
        var accessor = new HttpContextAccessor { HttpContext = context };
        return new HttpContextService(accessor);
    }

    [Fact]
    public void GetOriginalProtocol_ShouldRead_FromCfVisitor_WhenPresent()
    {
        var ctx = new DefaultHttpContext();
        // Note: header key follows implementation constant 'Cf-Vistor'
        ctx.Request.Headers["Cf-Vistor"] = new StringValues("{\"scheme\":\"https\"}");
        var service = CreateService(ctx);

        service.GetOriginalProtocol().Should().Be("https");
    }

    [Fact]
    public void GetOriginalProtocol_ShouldRead_FromXForwardedProto_WhenPresent()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["X-Forwarded-Proto"] = new StringValues("https, http");
        var service = CreateService(ctx);

        service.GetOriginalProtocol().Should().Be("https");
    }

    [Fact]
    public void GetOriginalProtocol_ShouldReturn_RequestScheme_WhenHeadersInvalid()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Headers["Cf-Vistor"] = new StringValues("{invalid-json}");
        ctx.Request.Headers["X-Forwarded-Proto"] = new StringValues("invalid");
        var service = CreateService(ctx);

        service.GetOriginalProtocol().Should().Be("http");
    }

    [Fact]
    public void IsMobile_ShouldReturnFalse_WhenUserAgentTooShort()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["User-Agent"] = new StringValues("abc");
        var service = CreateService(ctx);

        service.IsMobile().Should().BeFalse();
    }

    [Fact]
    public void GetOriginalProtocol_ShouldFallback_ToRequestScheme()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        var service = CreateService(ctx);

        service.GetOriginalProtocol().Should().Be("http");
    }

    [Fact]
    public void GetDeviceName_ShouldReturn_SpecificDevice_WhenKnown()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["User-Agent"] = new StringValues("Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X)");
        var service = CreateService(ctx);

        service.GetDeviceName(ctx.Request.Headers["User-Agent"]).Should().Be("iPhone");
    }

    [Fact]
    public void GetDeviceName_ShouldReturn_Mobile_WhenUnknownButMobileUA()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["User-Agent"] = new StringValues("Android Mobile");
        var service = CreateService(ctx);

        service.GetDeviceName("something").Should().Be("Mobile");
    }

    [Fact]
    public void GetDeviceName_ShouldReturn_Computer_WhenNotMobile()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["User-Agent"] = new StringValues("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        var service = CreateService(ctx);

        service.GetDeviceName(ctx.Request.Headers["User-Agent"]).Should().Be("Computer/Laptop");
    }

    [Fact]
    public void IsMobile_ShouldDetect_FromKeywords()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["User-Agent"] = new StringValues("Opera Mini on mobile");
        var service = CreateService(ctx);

        service.IsMobile().Should().BeTrue();
    }

    [Fact]
    public void GetCountry_ShouldUse_AcceptLanguage()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Accept-Language"] = new StringValues("en-US,en;q=0.9");
        var service = CreateService(ctx);

        service.GetCountry().Should().Be("United States");
    }
}


