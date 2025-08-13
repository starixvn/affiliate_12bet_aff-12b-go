namespace Fanex.Affiliate.Link.Services.HealthCheck
{
    using Constants;
    using Fanex.Affiliate.Link.Helpers;
    using Models.HealthCheck;
    using Services.Interface;

    /// <summary>
    /// In-memory implementation of health check history tracking
    /// In production, this should be replaced with persistent storage
    /// </summary>
    public class HealthCheckHistoryService : IHealthCheckHistoryService
    {
        private readonly IMemoryCacheService _cacheService;
        private readonly ILogger<HealthCheckHistoryService> _logger;
        private const string HISTORY_CACHE_KEY = CacheKey.MASTER_DATA + ".HealthHistory";
        private const int MAX_HISTORY_RECORDS = 1000;
        private const int CacheDurationSeconds = 86400; // 24 hours
        private const int MaxRecentErrors = 5;

        public HealthCheckHistoryService(
            IMemoryCacheService cacheService,
            ILogger<HealthCheckHistoryService> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task RecordHealthCheckAsync(HealthStatus healthStatus)
        {
            try
            {
                var historyRecord = new HealthCheckHistory
                {
                    Timestamp = healthStatus.Timestamp,
                    OverallStatus = healthStatus.Status,
                    TotalDuration = healthStatus.TotalDuration,
                    Components = healthStatus.Components.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new ComponentHealthHistory
                        {
                            Name = kvp.Value.Name,
                            Status = kvp.Value.Status,
                            ResponseTime = kvp.Value.ResponseTime,
                            ErrorMessage = kvp.Value.ErrorMessage
                        }),
                    Metrics = new SystemMetricsHistory
                    {
                        MemoryUsedBytes = healthStatus.Metrics.MemoryUsedBytes,
                        ActiveThreads = healthStatus.Metrics.ActiveThreads,
                        Uptime = healthStatus.Metrics.Uptime,
                        CpuUsagePercent = healthStatus.Metrics.CpuUsagePercent
                    }
                };

                var history = GetHistoryFromCache() ?? new List<HealthCheckHistory>();
                history.Add(historyRecord);

                // Keep only the most recent records
                if (history.Count > MAX_HISTORY_RECORDS)
                {
                    history = history.OrderByDescending(h => h.Timestamp)
                                   .Take(MAX_HISTORY_RECORDS)
                                   .ToList();
                }

                _cacheService.Set(HISTORY_CACHE_KEY, history, CacheDurationSeconds); // Cache for 24 hours

                _logger.LogDebug("Recorded health check history: {Status} at {Timestamp}",
                    healthStatus.Status, healthStatus.Timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record health check history");
            }

            return Task.CompletedTask;
        }

        public Task<List<HealthCheckHistory>> GetHistoryAsync(DateTime startTime, DateTime endTime, int maxRecords = 100)
        {
            try
            {
                var history = GetHistoryFromCache() ?? new List<HealthCheckHistory>();

                return Task.FromResult(history
                    .Where(h => h.Timestamp >= startTime && h.Timestamp <= endTime)
                    .OrderByDescending(h => h.Timestamp)
                    .Take(maxRecords)
                    .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get health check history");
                return Task.FromResult(new List<HealthCheckHistory>());
            }
        }

        public async Task<HealthCheckSummary> GetSummaryAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                var history = await GetHistoryAsync(startTime, endTime, int.MaxValue);

                if (!history.Any())
                {
                    return new HealthCheckSummary
                    {
                        StartTime = startTime,
                        EndTime = endTime
                    };
                }

                var summary = new HealthCheckSummary
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalChecks = history.Count,
                    HealthyChecks = history.Count(h => h.OverallStatus == "Healthy"),
                    WarningChecks = history.Count(h => h.OverallStatus == "Warning"),
                    UnhealthyChecks = history.Count(h => h.OverallStatus == "Unhealthy")
                };

                // Calculate component summaries
                var componentNames = history.SelectMany(h => h.Components.Keys).Distinct();
                foreach (var componentName in componentNames)
                {
                    var componentHistory = history
                        .Where(h => h.Components.ContainsKey(componentName))
                        .Select(h => h.Components[componentName])
                        .ToList();

                    summary.ComponentSummaries[componentName] = new ComponentSummary
                    {
                        ComponentName = componentName,
                        TotalChecks = componentHistory.Count,
                        HealthyChecks = componentHistory.Count(c => c.Status == "Healthy"),
                        WarningChecks = componentHistory.Count(c => c.Status == "Warning"),
                        UnhealthyChecks = componentHistory.Count(c => c.Status == "Unhealthy"),
                        AverageResponseTimeMs = componentHistory.Average(c => c.ResponseTime.TotalMilliseconds),
                        RecentErrors = componentHistory
                            .Where(c => !string.IsNullOrEmpty(c.ErrorMessage))
                            .Select(c => c.ErrorMessage)
                            .Distinct()
                            .Take(MaxRecentErrors)
                            .ToList()
                    };
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate health check summary");
                return new HealthCheckSummary { StartTime = startTime, EndTime = endTime };
            }
        }

        public async Task<List<ComponentSummary>> GetComponentTrendsAsync(string componentName, DateTime startTime, DateTime endTime)
        {
            try
            {
                var history = await GetHistoryAsync(startTime, endTime, int.MaxValue);
                var componentHistory = history
                    .Where(h => h.Components.ContainsKey(componentName))
                    .Select(h => h.Components[componentName])
                    .ToList();

                // Group by hour for trending
                var hourlyGroups = componentHistory
                    .GroupBy(c => DateTimeHelper.CreateDateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour))
                    .ToList();

                return hourlyGroups.Select(group => new ComponentSummary
                {
                    ComponentName = componentName,
                    TotalChecks = group.Count(),
                    HealthyChecks = group.Count(c => c.Status == "Healthy"),
                    WarningChecks = group.Count(c => c.Status == "Warning"),
                    UnhealthyChecks = group.Count(c => c.Status == "Unhealthy"),
                    AverageResponseTimeMs = group.Average(c => c.ResponseTime.TotalMilliseconds)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get component trends for {Component}", componentName);
                return new List<ComponentSummary>();
            }
        }

        public Task CleanupOldRecordsAsync(TimeSpan retentionPeriod)
        {
            try
            {
                var history = GetHistoryFromCache() ?? new List<HealthCheckHistory>();
                var cutoffTime = DateTimeHelper.ServerTime - retentionPeriod;

                var filteredHistory = history.Where(h => h.Timestamp >= cutoffTime).ToList();

                if (filteredHistory.Count != history.Count)
                {
                    _cacheService.Set(HISTORY_CACHE_KEY, filteredHistory, CacheDurationSeconds);
                    _logger.LogInformation("Cleaned up {Count} old health check records",
                        history.Count - filteredHistory.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old health check records");
            }

            return Task.CompletedTask;
        }

        public async Task<Dictionary<string, object>> GetRealTimeMetricsAsync()
        {
            try
            {
                var last24Hours = DateTimeHelper.ServerTimeOffset.AddHours(-24);
                var summary = await GetSummaryAsync(last24Hours.DateTime, DateTimeHelper.ServerTime);

                return new Dictionary<string, object>
                {
                    ["totalChecks24h"] = summary.TotalChecks,
                    ["uptimePercent24h"] = summary.UptimePercent,
                    ["healthyChecks24h"] = summary.HealthyChecks,
                    ["warningChecks24h"] = summary.WarningChecks,
                    ["unhealthyChecks24h"] = summary.UnhealthyChecks,
                    ["componentSummaries"] = summary.ComponentSummaries,
                    ["lastUpdated"] = DateTimeHelper.ServerTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get real-time metrics");
                return new Dictionary<string, object>();
            }
        }

        private List<HealthCheckHistory> GetHistoryFromCache()
        {
            return _cacheService.Get<List<HealthCheckHistory>>(HISTORY_CACHE_KEY);
        }
    }
}