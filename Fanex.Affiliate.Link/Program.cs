using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Services;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Affiliate.Link.Extensions;
using Fanex.Affiliate.Link.Middleware;
using Fanex.Affiliate.Link.Models.HealthCheck;
using Fanex.Affiliate.Link.Services.HealthCheck;
using Fanex.Affiliate.Link.Services.Impl;
using Fanex.Affiliate.Link.Services.Interface;
using Fanex.Affiliate.Link.Workers;
using Fanex.Data;
using Fanex.Data.Repository;
using ONEbook.UM.DependencyInjection;
using ONEbook.UM.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Configure logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Register custom services
builder.Services.AddScoped<IClickTrackingEventService, ClickTrackingEventService>();
builder.Services.AddScoped<IAffInfoService, AffInfoService>();
builder.Services.AddScoped<IMemoryCacheService, MemoryCacheService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IHttpContextService, HttpContextService>();
builder.Services.AddSingleton<IDynamicRepository, DynamicRepository>();

// Register health check services
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
builder.Services.AddScoped<IHealthCheckConfigurationService, HealthCheckConfigurationService>();
builder.Services.AddScoped<IHealthCheckHistoryService, HealthCheckHistoryService>();
builder.Services.AddScoped<IHealthCheckServerInfoService, HealthCheckServerInfoService>();

// Configure health check settings
builder.Services.Configure<HealthCheckConfig>(
    builder.Configuration.GetSection("HealthCheck"));

// Configure configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

RedisConnectionHelper.Initialize(builder.Configuration);
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => RedisConnectionHelper.Connection);
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<Fanex.Logging.ILogger, Fanex.Logging.Logger>();
builder.Services.AddSingleton<ILogService, LogService>();

builder.Services.AddHostedService<RabbitMqBusHostedService>();
builder.Services.AddSingleton<IRabbitBus, Fanex.Affiliate.Link.Core.Services.RabbitBus>();
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

builder.Services.AddOneBookUmForService(builder.Configuration);

// Configure thread pool settings
const short DefaultWorkerThreads = 300;
const short DefaultCompletionPortThreads = 300;
_ = ThreadPool.SetMinThreads(DefaultWorkerThreads, DefaultCompletionPortThreads);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Custom middleware for handling affiliate link 404 errors with detailed messages
app.UseMiddleware<AffiliateErrorMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure affiliate link routes using centralized extension method
app.MapAffiliateRoutes();

// Initialize Fanex Data
DbSettingProviderManager
    .StartNewSession()
    .Register(builder.Configuration)
    .Run();

// Register Sentry logging
Fanex.Affiliate.Link.LoggingConfig.RegisterSentry();

app.Run();