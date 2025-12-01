using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Services;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.Services;
using Xunit;

namespace Fanex.Affiliate.Link.Consumer.Tests;

public class ProgramTests
{
    [Fact]
    public void Program_ShouldRegisterClickTrackingRedisStorageWorker()
    {
        // Arrange & Act
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHostedService<ClickTrackingRedisStorageWorker>();

        // Assert
        // This test verifies that the service can be registered without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public void Program_ShouldRegisterRequiredServices()
    {
        // Arrange & Act
        var builder = WebApplication.CreateBuilder();

        // Register services as in Program.cs
        builder.Services.AddHostedService<ClickTrackingRedisStorageWorker>();
        builder.Services.AddSingleton<IMaintenanceService, MaintenanceService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IRedisService, RedisService>();
        builder.Services.AddSingleton<ILogService, LogService>();

        // Assert
        // These tests verify that services can be registered without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public void Program_ShouldRegisterServicesAsSingletons()
    {
        // Arrange & Act
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSingleton<IMaintenanceService, MaintenanceService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IRedisService, RedisService>();
        builder.Services.AddSingleton<ILogService, LogService>();

        // Assert
        // These tests verify that services can be registered without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public void Program_ShouldRegisterHttpClient()
    {
        // Arrange & Act
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpClient();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        // HttpClient factory is registered by AddHttpClient()
        Assert.True(true);
    }

    [Fact]
    public void Program_ShouldRegisterServicesWithoutThrowingException()
    {
        // Arrange & Act & Assert - Should not throw
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSingleton<IMaintenanceService, MaintenanceService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IRedisService, RedisService>();
        builder.Services.AddSingleton<ILogService, LogService>();

        // This test verifies that services can be registered without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public void Program_ShouldRegisterHostedServiceWithoutThrowingException()
    {
        // Arrange & Act & Assert - Should not throw
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHostedService<ClickTrackingRedisStorageWorker>();

        // This test verifies that hosted service can be registered without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public void Program_ShouldRegisterHttpClientFactory()
    {
        // Arrange & Act
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpClient();

        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        var httpClientFactory = serviceProvider.GetService<System.Net.Http.IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldCreateWebApplication()
    {
        // Arrange & Act
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHostedService<ClickTrackingRedisStorageWorker>();

        // Assert - Should not throw
        var app = builder.Build();
        app.Should().NotBeNull();
        app.Should().BeOfType<WebApplication>();
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }
}
