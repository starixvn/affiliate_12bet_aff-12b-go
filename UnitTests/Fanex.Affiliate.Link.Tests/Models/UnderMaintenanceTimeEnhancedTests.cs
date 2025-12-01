using Fanex.Affiliate.Link.Models;
using FluentAssertions;
using System;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Models;

public class UnderMaintenanceTimeEnhancedTests
{
    [Fact]
    public void UnderMaintenanceTime_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var maintenanceTime = new UnderMaintenanceTime();

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeFalse();
        maintenanceTime.Message.Should().Be(string.Empty);
        maintenanceTime.StartTime.Should().BeNull();
        maintenanceTime.EndTime.Should().BeNull();
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime();
        var startTime = new DateTime(2023, 12, 25, 10, 0, 0);
        var endTime = new DateTime(2023, 12, 25, 14, 0, 0);
        var message = "Scheduled maintenance";

        // Act
        maintenanceTime.IsUnderMaintenance = true;
        maintenanceTime.Message = message;
        maintenanceTime.StartTime = startTime;
        maintenanceTime.EndTime = endTime;

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();
        maintenanceTime.Message.Should().Be(message);
        maintenanceTime.StartTime.Should().Be(startTime);
        maintenanceTime.EndTime.Should().Be(endTime);
    }

    [Theory]
    [InlineData(true, "System under maintenance")]
    [InlineData(false, "")]
    [InlineData(true, "Planned upgrade")]
    [InlineData(false, "Normal operation")]
    public void UnderMaintenanceTime_ShouldAcceptVariousValues(bool isUnderMaintenance, string message)
    {
        // Arrange & Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = isUnderMaintenance,
            Message = message
        };

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().Be(isUnderMaintenance);
        maintenanceTime.Message.Should().Be(message);
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldHandleMaintenanceWindow()
    {
        // Arrange
        var now = DateTime.Now;
        var startTime = now.AddHours(-1);
        var endTime = now.AddHours(1);

        // Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = true,
            Message = "Maintenance in progress",
            StartTime = startTime,
            EndTime = endTime
        };

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();
        maintenanceTime.Message.Should().Be("Maintenance in progress");
        maintenanceTime.StartTime.Should().Be(startTime);
        maintenanceTime.EndTime.Should().Be(endTime);
        maintenanceTime.EndTime.Should().BeAfter(maintenanceTime.StartTime.Value);
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldHandleNullableDateTimes()
    {
        // Arrange & Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = false,
            StartTime = null,
            EndTime = null
        };

        // Assert
        maintenanceTime.StartTime.Should().BeNull();
        maintenanceTime.EndTime.Should().BeNull();
        maintenanceTime.StartTime.HasValue.Should().BeFalse();
        maintenanceTime.EndTime.HasValue.Should().BeFalse();
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldAllowPartialTimeSettings()
    {
        // Arrange
        var startTime = DateTime.Now;

        // Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = true,
            StartTime = startTime,
            EndTime = null
        };

        // Assert
        maintenanceTime.StartTime.Should().Be(startTime);
        maintenanceTime.EndTime.Should().BeNull();
    }

    [Fact]
    public void UnderMaintenanceTime_Properties_ShouldBeIndependent()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime();

        // Act & Assert
        maintenanceTime.IsUnderMaintenance = true;
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();
        maintenanceTime.Message.Should().Be(string.Empty);

        maintenanceTime.Message = "Test message";
        maintenanceTime.Message.Should().Be("Test message");
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();

        var testTime = DateTime.Now;
        maintenanceTime.StartTime = testTime;
        maintenanceTime.StartTime.Should().Be(testTime);
        maintenanceTime.EndTime.Should().BeNull();
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldHandleNullMessage()
    {
        // Arrange & Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            Message = null
        };

        // Assert - The setter will use the null value, but the default is string.Empty
        maintenanceTime.Message.Should().BeNull();
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldSupportObjectInitializer()
    {
        // Arrange
        var startTime = new DateTime(2024, 1, 1, 2, 0, 0);
        var endTime = new DateTime(2024, 1, 1, 6, 0, 0);

        // Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = true,
            Message = "Scheduled system upgrade",
            StartTime = startTime,
            EndTime = endTime
        };

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();
        maintenanceTime.Message.Should().Be("Scheduled system upgrade");
        maintenanceTime.StartTime.Should().Be(startTime);
        maintenanceTime.EndTime.Should().Be(endTime);
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldHandleEmptyMessage()
    {
        // Arrange & Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = true,
            Message = ""
        };

        // Assert
        maintenanceTime.IsUnderMaintenance.Should().BeTrue();
        maintenanceTime.Message.Should().Be("");
    }

    [Fact]
    public void UnderMaintenanceTime_ShouldHandleTimeZoneVariations()
    {
        // Arrange
        var utcTime = DateTime.UtcNow;
        var localTime = DateTime.Now;

        // Act
        var maintenanceTime = new UnderMaintenanceTime
        {
            StartTime = utcTime,
            EndTime = localTime
        };

        // Assert
        maintenanceTime.StartTime.Should().Be(utcTime);
        maintenanceTime.EndTime.Should().Be(localTime);
    }
}
