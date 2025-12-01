using Fanex.Affiliate.Link.Models.HealthCheck;
using FluentAssertions;
using System;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Models.HealthCheck;

public class HealthStatusTests
{
    [Fact]
    public void HealthStatus_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var healthStatus = new HealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.Now,
            TotalDuration = TimeSpan.FromSeconds(5)
        };

        // Assert
        healthStatus.Status.Should().Be("Healthy");
        healthStatus.Timestamp.Should().NotBe(DateTime.MinValue);
        healthStatus.TotalDuration.Should().Be(TimeSpan.FromSeconds(5));
        healthStatus.Components.Should().NotBeNull();
        healthStatus.Metrics.Should().NotBeNull();
    }

    [Fact]
    public void HealthStatus_ShouldHandleDefaultValues()
    {
        // Arrange
        var healthStatus = new HealthStatus();

        // Assert
        healthStatus.Status.Should().Be("Unknown");
        healthStatus.Timestamp.Should().NotBe(DateTime.MinValue);
        healthStatus.TotalDuration.Should().Be(TimeSpan.Zero);
        healthStatus.Components.Should().NotBeNull();
        healthStatus.Metrics.Should().NotBeNull();
    }

    [Fact]
    public void IsHealthy_ShouldReturnTrue_WhenStatusIsHealthy()
    {
        // Arrange
        var healthStatus = new HealthStatus { Status = "Healthy" };

        // Act
        var result = healthStatus.IsHealthy;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHealthy_ShouldReturnFalse_WhenStatusIsNotHealthy()
    {
        // Arrange
        var healthStatus = new HealthStatus { Status = "Unhealthy" };

        // Act
        var result = healthStatus.IsHealthy;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Summary_ShouldReturnCorrectFormat_WhenComponentsExist()
    {
        // Arrange
        var healthStatus = new HealthStatus();
        healthStatus.Components.Add("Component1", new ComponentHealth { Status = "Healthy" });
        healthStatus.Components.Add("Component2", new ComponentHealth { Status = "Unhealthy" });

        // Act
        var result = healthStatus.Summary;

        // Assert
        result.Should().Be("1/2 components healthy");
    }

    [Fact]
    public void Summary_ShouldReturnZeroComponents_WhenNoComponents()
    {
        // Arrange
        var healthStatus = new HealthStatus();

        // Act
        var result = healthStatus.Summary;

        // Assert
        result.Should().Be("0/0 components healthy");
    }

    [Fact]
    public void HealthStatus_ShouldBeMutable()
    {
        // Arrange
        var healthStatus = new HealthStatus();

        // Act
        healthStatus.Status = "Degraded";
        healthStatus.TotalDuration = TimeSpan.FromMinutes(10);

        // Assert
        healthStatus.Status.Should().Be("Degraded");
        healthStatus.TotalDuration.Should().Be(TimeSpan.FromMinutes(10));
    }
}
