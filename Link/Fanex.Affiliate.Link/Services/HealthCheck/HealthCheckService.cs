namespace Fanex.Affiliate.Link.Services.HealthCheck
{
    using Constants;
    using EasyNetQ;
    using Fanex.Affiliate.Link.Core.Services.Interfaces;
    using Fanex.Data.Repository;
    using Models.HealthCheck;
    using ONEbook.UM.Services;
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// Comprehensive health check service implementation
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IRedisService _redisService;
        private readonly IBus _rabbitMqBus;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IHealthCheckConfigurationService _configService;
        private readonly IHealthCheckHistoryService _historyService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthCheckService> _logger;
        private readonly DateTime _startTime;

        public HealthCheckService(
            IDynamicRepository dynamicRepository,
            IRedisService redisService,
            IBus rabbitMqBus,
            IMaintenanceService maintenanceService,
            IHealthCheckConfigurationService configService,
            IHealthCheckHistoryService historyService,
            IConfiguration configuration,
            ILogger<HealthCheckService> logger)
        {
            _dynamicRepository = dynamicRepository ?? throw new ArgumentNullException(nameof(dynamicRepository));
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
            _rabbitMqBus = rabbitMqBus ?? throw new ArgumentNullException(nameof(rabbitMqBus));
            _maintenanceService = maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _startTime = DateTimeOffset.UtcNow.DateTime;
        }

        public async Task<HealthStatus> GetHealthStatusAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var healthStatus = new HealthStatus();

            try
            {
                // Run all health checks in parallel for better performance
                var healthTasks = new Dictionary<string, Task<ComponentHealth>>
                {
                    ["database"] = CheckDatabaseHealthAsync(),
                    ["redis"] = CheckRedisHealthAsync(),
                    ["rabbitmq"] = CheckRabbitMQHealthAsync(),
                    ["route_constraints"] = CheckRouteConstraintPerformanceAsync(),
                    ["maintenance_service"] = CheckUnderMaintenanceServiceAsync()
                };

                await Task.WhenAll(healthTasks.Values);

                // Collect results
                foreach (var task in healthTasks)
                {
                    healthStatus.Components[task.Key] = await task.Value;
                }

                // Get system metrics
                healthStatus.Metrics = await GetSystemMetricsAsync();

                // Determine overall status
                healthStatus.Status = DetermineOverallStatus(healthStatus.Components.Values);
                healthStatus.TotalDuration = stopwatch.Elapsed;

                // Record history and check for alerts
                _ = Task.Run(async () =>
                {
                    await _historyService.RecordHealthCheckAsync(healthStatus);
                    await CheckForAlertsAsync(healthStatus);
                });

                _logger.LogInformation("Health check completed in {Duration}ms with status {Status}",
                    stopwatch.ElapsedMilliseconds, healthStatus.Status);

                return healthStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform health check");
                healthStatus.Status = "Unhealthy";
                healthStatus.TotalDuration = stopwatch.Elapsed;
                return healthStatus;
            }
        }

        public async Task<ComponentHealth> CheckDatabaseHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            // Test connect Database by the dynamic repository (external API service)
            try
            {
                var testCriteria = new Criterias.BaseCriteria("Aff_Market_Sel");
                var result = await _dynamicRepository.FetchAsync<dynamic>(testCriteria);

                stopwatch.Stop();

                if (result != null)
                {
                    return ComponentHealth.Healthy("Database",
                        "Database connection via DynamicRepository successful", stopwatch.Elapsed);
                }

                return ComponentHealth.Warning("Database",
                    "Database connected but returned unexpected result", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database health check failed");
                return ComponentHealth.Unhealthy("Database", ex.Message, stopwatch.Elapsed);
            }
        }

        public async Task<ComponentHealth> CheckRedisHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var testKey = $"health_check_{Guid.NewGuid()}";
                var testValue = $"test_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

                await _redisService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(30));
                var retrievedValue = await _redisService.GetAsync<string>(testKey);

                stopwatch.Stop();

                if (retrievedValue == testValue)
                {
                    // Clean up test key
                    try { await _redisService.RemoveAsync(testKey); } catch { /* Ignore cleanup errors */ }

                    return ComponentHealth.Healthy("Redis",
                        $"Redis cache responding correctly (response time: {stopwatch.ElapsedMilliseconds}ms)", stopwatch.Elapsed);
                }

                return ComponentHealth.Warning("Redis",
                    $"Redis cache test failed - expected '{testValue}', got '{retrievedValue}'", stopwatch.Elapsed);
            }
            catch (TimeoutException)
            {
                stopwatch.Stop();
                return ComponentHealth.Warning("Redis",
                    "Redis connection timeout (may be overloaded)", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogDebug(ex, "Redis health check failed");
                return ComponentHealth.Warning("Redis",
                    $"Redis connection issue: {ex.Message}", stopwatch.Elapsed);
            }
        }

        public Task<ComponentHealth> CheckRabbitMQHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Check RabbitMQ connection status
                var isConnected = _rabbitMqBus.Advanced.IsConnected;
                stopwatch.Stop();

                if (isConnected)
                {
                    return Task.FromResult(ComponentHealth.Healthy("RabbitMQ",
                        "RabbitMQ connection is active", stopwatch.Elapsed));
                }

                return Task.FromResult(ComponentHealth.Warning("RabbitMQ",
                    "RabbitMQ connection is not active", stopwatch.Elapsed));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ health check failed");
                return Task.FromResult(ComponentHealth.Unhealthy("RabbitMQ", ex.Message, stopwatch.Elapsed));
            }
        }

        public Task<ComponentHealth> CheckRouteConstraintPerformanceAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Test route constraint performance with sample URLs
                var testUrls = new[]
                {
                    "home/12345/en",
                    "12345/home/en",
                    "12sports/cricket/12345/en/mobile",
                    "invalid/pattern/test"
                };

                var results = new List<bool>();
                foreach (var url in testUrls)
                {
                    var segments = url.Split('/');
                    if (segments.Length >= 3)
                    {
                        var isNumeric = RegexPatterns.Numeric().IsMatch(segments[0]);
                        var isPageCode = RegexPatterns.PageCode().IsMatch(segments[1]);
                        results.Add(isNumeric || isPageCode);
                    }
                }

                stopwatch.Stop();

                var avgTime = stopwatch.Elapsed.TotalMilliseconds / testUrls.Length;

                if (avgTime < 1.0) // Should be sub-millisecond
                {
                    return Task.FromResult(ComponentHealth.Healthy("Route Constraints",
                        $"Average validation time: {avgTime:F3}ms", stopwatch.Elapsed));
                }
                else if (avgTime < 5.0)
                {
                    return Task.FromResult(ComponentHealth.Warning("Route Constraints",
                        $"Validation slower than expected: {avgTime:F3}ms", stopwatch.Elapsed));
                }

                return Task.FromResult(ComponentHealth.Unhealthy("Route Constraints",
                    $"Validation too slow: {avgTime:F3}ms", stopwatch.Elapsed));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Route constraint performance check failed");
                return Task.FromResult(ComponentHealth.Unhealthy("Route Constraints", ex.Message, stopwatch.Elapsed));
            }
        }

        public Task<ComponentHealth> CheckUnderMaintenanceServiceAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var maintenanceTime = _maintenanceService.GetUnderMaintenanceTime();
                stopwatch.Stop();

                var status = maintenanceTime.IsUnderMaintenanceTime ? "Warning" : "Healthy";
                var description = maintenanceTime.IsUnderMaintenanceTime
                    ? "System is currently under maintenance"
                    : "System is operational";

                return Task.FromResult(new ComponentHealth
                {
                    Name = "Maintenance Service",
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        ["isUnderMaintenance"] = maintenanceTime.IsUnderMaintenanceTime,
                        ["maintenanceWindow"] = maintenanceTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Under maintenance service health check failed");
                return Task.FromResult(ComponentHealth.Unhealthy("Maintenance Service", ex.Message, stopwatch.Elapsed));
            }
        }

        public Task<SystemMetrics> GetSystemMetricsAsync()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var assembly = Assembly.GetExecutingAssembly();

                return Task.FromResult(new SystemMetrics
                {
                    MemoryUsedBytes = process.WorkingSet64,
                    ActiveThreads = process.Threads.Count,
                    Uptime = DateTimeOffset.UtcNow - _startTime,
                    Version = assembly.GetName().Version?.ToString() ?? "Unknown",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    AdditionalMetrics = new Dictionary<string, object>
                    {
                        ["ProcessId"] = process.Id,
                        ["MachineName"] = Environment.MachineName,
                        ["ProcessorCount"] = Environment.ProcessorCount,
                        ["WorkingSet"] = process.WorkingSet64,
                        ["VirtualMemorySize"] = process.VirtualMemorySize64,
                        ["GCTotalMemory"] = GC.GetTotalMemory(false)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to collect system metrics");
                return Task.FromResult(new SystemMetrics());
            }
        }

        private static string DetermineOverallStatus(IEnumerable<ComponentHealth> components)
        {
            var componentsList = components.ToList();

            if (componentsList.Any(c => c.IsUnhealthy))
                return "Unhealthy";

            if (componentsList.Any(c => c.IsWarning))
                return "Warning";

            return componentsList.All(c => c.IsHealthy) ? "Healthy" : "Unknown";
        }

        private async Task CheckForAlertsAsync(HealthStatus healthStatus)
        {
            try
            {
                if (!_configService.GetConfiguration().EnableAlerts)
                    return;

                var thresholds = _configService.GetThresholds();

                // Check for unhealthy components
                foreach (var component in healthStatus.Components.Where(c => c.Value.IsUnhealthy))
                {
                    await _configService.SendAlertAsync(
                        component.Key,
                        $"Component is unhealthy: {component.Value.ErrorMessage}",
                        "Critical");
                }

                // Check for performance thresholds
                if (healthStatus.Metrics.MemoryUsedBytes / 1024 / 1024 > thresholds.MemoryUsageMB)
                {
                    await _configService.SendAlertAsync(
                        "system",
                        $"High memory usage: {healthStatus.Metrics.MemoryUsedBytes / 1024 / 1024}MB",
                        "Warning");
                }

                if (healthStatus.Metrics.ActiveThreads > thresholds.ActiveThreadsCritical)
                {
                    await _configService.SendAlertAsync(
                        "system",
                        $"Critical thread count: {healthStatus.Metrics.ActiveThreads}",
                        "Critical");
                }
                else if (healthStatus.Metrics.ActiveThreads > thresholds.ActiveThreadsWarning)
                {
                    await _configService.SendAlertAsync(
                        "system",
                        $"High thread count: {healthStatus.Metrics.ActiveThreads}",
                        "Warning");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for alerts");
            }
        }
    }
}