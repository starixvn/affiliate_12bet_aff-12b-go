using Fanex.Affiliate.Link.Models.HealthCheck;
using Fanex.Affiliate.Link.Services.HealthCheck;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Services.HealthCheck;

public class HealthCheckConfigurationServiceTests
{
    private static HealthCheckConfigurationService Create(HealthCheckConfig cfg)
    {
        var options = Options.Create(cfg);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<HealthCheckConfigurationService>>();
        var dummyConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        return new HealthCheckConfigurationService(options, logger.Object, dummyConfig.Object);
    }

    [Fact]
    public void IsComponentEnabled_ShouldRespect_Config()
    {
        var cfg = new HealthCheckConfig
        {
            ComponentsEnabled = new Dictionary<string, bool> { ["redis"] = true, ["database"] = false }
        };
        var svc = Create(cfg);

        svc.IsComponentEnabled("redis").Should().BeTrue();
        svc.IsComponentEnabled("database").Should().BeFalse();
    }

    [Fact]
    public void ValidateApiKey_ShouldReturnTrue_WhenAuthDisabled()
    {
        var cfg = new HealthCheckConfig { RequireAuthentication = false };
        var svc = Create(cfg);

        svc.ValidateApiKey(null).Should().BeTrue();
    }

    [Fact]
    public void ValidateApiKey_ShouldMatch_WhenAuthEnabled()
    {
        var cfg = new HealthCheckConfig { RequireAuthentication = true, ApiKey = "abc" };
        var svc = Create(cfg);

        svc.ValidateApiKey("abc").Should().BeTrue();
        svc.ValidateApiKey("xyz").Should().BeFalse();
    }

    [Fact]
    public async Task SendAlertAsync_ShouldReturnFalse_WhenAlertsDisabled()
    {
        var cfg = new HealthCheckConfig { EnableAlerts = false };
        var svc = Create(cfg);

        var result = await svc.SendAlertAsync("db", "msg", "Warning");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAlertAsync_ShouldReturnTrue_WhenAlertsEnabled()
    {
        var cfg = new HealthCheckConfig { EnableAlerts = true };
        var svc = Create(cfg);

        var result = await svc.SendAlertAsync("db", "msg", "Warning");
        result.Should().BeTrue();
    }
}


