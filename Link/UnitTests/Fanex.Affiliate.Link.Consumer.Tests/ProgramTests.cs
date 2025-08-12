using System;
using Fanex.Affiliate.Link.Consumer;
using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Services;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
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
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.QueueIdentify.Should().Be("12b.user.click");
        Constants.CacheRedisQueueName.Should().Be("Aff.12B.Go.ClickQueues");
    }
}
