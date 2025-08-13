using Fanex.Affiliate.Link.Models;
using FluentAssertions;
using Xunit;
using System;

namespace Fanex.Affiliate.Link.Tests.Models;

public class UnderMaintenanceTimeTests
{
    [Fact]
    public void UnderMaintenanceTime_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = true,
            Message = "System is under maintenance",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2)
        };

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();
        maintenanceTime.Message.Should().Be("System is under maintenance");
        maintenanceTime.StartTime.Should().NotBeNull();
        maintenanceTime.EndTime.Should().NotBeNull();
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldHandleNullValues()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = false,
            Message = null,
            StartTime = null,
            EndTime = null
        };

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeFalse();
        maintenanceTime.Message.Should().BeNull();
        maintenanceTime.StartTime.Should().BeNull();
        maintenanceTime.EndTime.Should().BeNull();
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldHandleDefaultValues()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime();

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeFalse();
        maintenanceTime.Message.Should().Be(""); // Default value is empty string, not null
        maintenanceTime.StartTime.Should().BeNull();
        maintenanceTime.EndTime.Should().BeNull();
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldBeMutable()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime();

        // Act
        maintenanceTime.IsUnderMaintenance = true;
        maintenanceTime.Message = "Updated message";
        maintenanceTime.StartTime = DateTime.Now;
        maintenanceTime.EndTime = DateTime.Now.AddHours(1);

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();
        maintenanceTime.Message.Should().Be("Updated message");
        maintenanceTime.StartTime.Should().NotBeNull();
        maintenanceTime.EndTime.Should().NotBeNull();
    }
}