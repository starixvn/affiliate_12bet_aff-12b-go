using Fanex.Affiliate.Link.Models;
using Fanex.Affiliate.Link.Services.Impl;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Services;

public class UnderMaintenanceServiceTests
{
    private readonly UnderMaintenanceService _underMaintenanceService;

    public UnderMaintenanceServiceTests()
    {
        _underMaintenanceService = new UnderMaintenanceService();
    }

    [Fact]
    public void GetUnderMaintenanceTime_ShouldReturnNotUnderMaintenance_WhenCalled()
    {
        // Act
        var result = _underMaintenanceService.GetUnderMaintenanceTime();

        // Assert
        result.Should().NotBeNull();
        result.IsUnderMaintenance.Should().BeFalse();
        result.Message.Should().Be("System is operational");
        result.StartTime.Should().BeNull();
        result.EndTime.Should().BeNull();
    }

    [Fact]
    public void IsUnderMaintenance_ShouldReturnFalse_WhenCalled()
    {
        // Act
        var result = _underMaintenanceService.IsUnderMaintenance();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetUnderMaintenanceTime_ShouldReturnConsistentResult_WhenCalledMultipleTimes()
    {
        // Act
        var result1 = _underMaintenanceService.GetUnderMaintenanceTime();
        var result2 = _underMaintenanceService.GetUnderMaintenanceTime();

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public void IsUnderMaintenance_ShouldReturnConsistentResult_WhenCalledMultipleTimes()
    {
        // Act
        var result1 = _underMaintenanceService.IsUnderMaintenance();
        var result2 = _underMaintenanceService.IsUnderMaintenance();

        // Assert
        result1.Should().Be(result2);
    }
}

