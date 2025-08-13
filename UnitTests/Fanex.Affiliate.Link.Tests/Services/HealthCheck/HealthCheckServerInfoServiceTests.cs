using Fanex.Affiliate.Link.Services.HealthCheck;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace Fanex.Affiliate.Link.Tests.Services.HealthCheck;

public class HealthCheckServerInfoServiceTests
{
    [Fact]
    public async Task GetServerInfoAsync_ShouldReturnDictionary_WhenHttpContextAvailable()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var env = new Mock<IHostEnvironment>();
        env.Setup(x => x.EnvironmentName).Returns("Test");
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<HealthCheckServerInfoService>>();

        var svc = new HealthCheckServerInfoService(accessor, env.Object, logger.Object);
        var info = await svc.GetServerInfoAsync();

        info.Should().NotBeNull();
        info.ContainsKey("Env").Should().BeTrue();
    }

    [Fact]
    public void GetBasicServerInfo_ShouldReturnMinimalDictionary()
    {
        var accessor = new HttpContextAccessor { HttpContext = null };
        var env = new Mock<IHostEnvironment>();
        env.Setup(x => x.EnvironmentName).Returns("Test");
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<HealthCheckServerInfoService>>();

        var svc = new HealthCheckServerInfoService(accessor, env.Object, logger.Object);
        var info = svc.GetBasicServerInfo();

        info.Should().NotBeNull();
        info.ContainsKey("Env").Should().BeTrue();
    }
}


