using EasyNetQ;
using Fanex.Affiliate.Link.Consumer;
using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Services;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using ONEbook.UM.DependencyInjection;
using ONEbook.UM.Services;
using SharpRaven;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ClickTrackingWorker>();
builder.Services.AddHostedService<ClickTrackingRedisStorageWorker>();
var rabbitMqConnectionString = builder.Configuration.GetValue<string>("RabbitMQConnectionString");
builder.Services.AddHttpClient();
builder.Services.AddSingleton(RabbitHutch.CreateBus(rabbitMqConnectionString));
builder.Services.AddOneBookUmForService(builder.Configuration);
builder.Services.AddSingleton<IMaintenanceService, MaintenanceService>();
builder.Services.AddSingleton<IDynamicRepository, DynamicRepository>();
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
RedisConnectionHelper.Initialize(builder.Configuration);
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => RedisConnectionHelper.Connection);
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<Fanex.Logging.ILogger, Fanex.Logging.Logger>();
builder.Services.AddSingleton<ILogService, Fanex.Affiliate.Link.Core.Services.LogService>();

var env = builder.Configuration["Sentry:Environment"];
var backEndDsn = builder.Configuration["Sentry:Dsn"];
var category = builder.Configuration["Sentry:LogCategory"];

LogManager.SetDefaultLogCategory(category).Use(new List<ILogEngine>
        {
            new SentryLogging(new SentryEngineOptions
            {
                Dsn = new Dsn(backEndDsn),
                Environment = env
            })
        });

DbSettingProviderManager
    .StartNewSession()
    .Register(builder.Configuration)
    .Run();

var host = builder.Build();
host.MapGet("/", () => "Affiliate.Link.Consumer running!");
host.MapGet("/sentry", async () =>
{
    var logService = host.Services.GetRequiredService<Fanex.Logging.ILogger>();
    await logService.ErrorAsync("Consumer Test Sentry");

    return "Consumer Test sentry!";
});

host.Run();
