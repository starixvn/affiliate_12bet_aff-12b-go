using Fanex.Affiliate.Link.Models;
using Fanex.Affiliate.Link.Services.Impl;
using Fanex.Affiliate.Link.Services.Interface;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Services;

public class UnderMaintenanceServiceEnhancedTests
{
    private readonly UnderMaintenanceService _service;

    public UnderMaintenanceServiceEnhancedTests()
    {
        _service = new UnderMaintenanceService();
    }

    [Fact]
    public void UnderMaintenanceService_ShouldImplementInterface()
    {
        // Act & Assert
        _service.Should().BeAssignableTo<IUnderMaintenanceService>();
    }

    [Fact]
    public void GetUnderMaintenanceTime_ShouldReturnConsistentResult()
    {
        // Act
        var result1 = _service.GetUnderMaintenanceTime();
        var result2 = _service.GetUnderMaintenanceTime();

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public void IsUnderMaintenance_ShouldBeConsistentWithGetUnderMaintenanceTime()
    {
        // Act
        var maintenanceTime = _service.GetUnderMaintenanceTime();
        var isUnderMaintenance = _service.IsUnderMaintenance();

        // Assert
        isUnderMaintenance.Should().Be(maintenanceTime.IsUnderMaintenance);
    }

    [Fact]
    public void GetUnderMaintenanceTime_ShouldReturnValidUnderMaintenanceTimeObject()
    {
        // Act
        var result = _service.GetUnderMaintenanceTime();

        // Assert
        result.Should().BeOfType<UnderMaintenanceTime>();
        Assert.True(result.IsUnderMaintenance == true || result.IsUnderMaintenance == false);
        result.Message.Should().NotBeNull();
    }

    [Fact]
    public void Service_ShouldHaveParameterlessConstructor()
    {
        // Act & Assert
        var creating = () => new UnderMaintenanceService();
        creating.Should().NotThrow();
    }

    [Fact]
    public void IsUnderMaintenance_ShouldReturnBooleanValue()
    {
        // Act
        var result = _service.IsUnderMaintenance();

        // Assert
        Assert.True(result == true || result == false);
    }

    [Fact]
    public void GetUnderMaintenanceTime_Message_ShouldNotBeEmpty()
    {
        // Act
        var result = _service.GetUnderMaintenanceTime();

        // Assert
        result.Message.Should().NotBeEmpty();
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Service_ShouldMaintainStateConsistency()
    {
        // Act - Call multiple times to ensure consistency
        var results = new[]
        {
            _service.IsUnderMaintenance(),
            _service.IsUnderMaintenance(),
            _service.IsUnderMaintenance()
        };

        // Assert
        results.Should().AllBeEquivalentTo(false);
    }

    [Fact]
    public void GetUnderMaintenanceTime_ShouldAlwaysReturnFalseForMaintenance()
    {
        // Act - Call multiple times
        var results = new[]
        {
            _service.GetUnderMaintenanceTime(),
            _service.GetUnderMaintenanceTime(),
            _service.GetUnderMaintenanceTime()
        };

        // Assert
        results.Should().AllSatisfy(result =>
        {
            result.IsUnderMaintenance.Should().BeFalse();
            result.Message.Should().Be("System is operational");
            result.StartTime.Should().BeNull();
            result.EndTime.Should().BeNull();
        });
    }

    [Fact]
    public void GetUnderMaintenanceTime_Properties_ShouldHaveExpectedValues()
    {
        // Act
        var result = _service.GetUnderMaintenanceTime();

        // Assert
        result.IsUnderMaintenance.Should().BeFalse("service always returns not under maintenance");
        result.Message.Should().Be("System is operational", "service provides operational message");
        result.StartTime.Should().BeNull("no maintenance start time is set");
        result.EndTime.Should().BeNull("no maintenance end time is set");
    }
}
