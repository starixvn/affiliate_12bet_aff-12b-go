using Fanex.Affiliate.Link.Models.HealthCheck;
using FluentAssertions;
using Xunit;
using System;

namespace Fanex.Affiliate.Link.Tests.Models.HealthCheck;

public class SystemMetricsTests
{
    [Fact]
    public void SystemMetrics_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var metrics = new SystemMetrics
        {
            CpuUsagePercent = 25.5,
            MemoryUsedBytes = 1024 * 1024 * 100,
            MemoryTotalBytes = 1024 * 1024 * 1000,
            ActiveThreads = 10,
            ActiveConnections = 5,
            Uptime = TimeSpan.FromHours(24),
            Version = "1.0.0",
            Environment = "Production"
        };

        // Assert
        metrics.CpuUsagePercent.Should().Be(25.5);
        metrics.MemoryUsedBytes.Should().Be(1024 * 1024 * 100);
        metrics.MemoryTotalBytes.Should().Be(1024 * 1024 * 1000);
        metrics.ActiveThreads.Should().Be(10);
        metrics.ActiveConnections.Should().Be(5);
        metrics.Uptime.Should().Be(TimeSpan.FromHours(24));
        metrics.Version.Should().Be("1.0.0");
        metrics.Environment.Should().Be("Production");
    }

    [Fact]
    public void MemoryUsagePercent_ShouldCalculateCorrectly()
    {
        // Arrange
        var metrics = new SystemMetrics
        {
            MemoryUsedBytes = 500 * 1024 * 1024,
            MemoryTotalBytes = 1000 * 1024 * 1024
        };

        // Act
        var result = metrics.MemoryUsagePercent;

        // Assert
        result.Should().Be(50.0);
    }

    [Fact]
    public void DiskUsagePercent_ShouldCalculateCorrectly()
    {
        // Arrange
        var metrics = new SystemMetrics
        {
            DiskUsedBytes = 50 * 1024 * 1024 * 1024L, // 50GB
            DiskTotalBytes = 250 * 1024 * 1024 * 1024L // 250GB
        };

        // Act
        var result = metrics.DiskUsagePercent;

        // Assert
        result.Should().Be(20.0);
    }
}
