using System;
using Fanex.Affiliate.Link.Controllers;
using Fanex.Affiliate.Link.Models;
using Fanex.Affiliate.Link.Services.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Controllers;

public class UnderMaintenanceControllerTests
{
    private readonly Mock<IUnderMaintenanceService> _mockUnderMaintenanceService;
    private readonly UnderMaintenanceController _controller;

    public UnderMaintenanceControllerTests()
    {
        _mockUnderMaintenanceService = new Mock<IUnderMaintenanceService>();
        _controller = new UnderMaintenanceController(_mockUnderMaintenanceService.Object);
    }

    [Fact]
    public void Index_ShouldReturnRedirectToActionResult_WhenNotUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = false,
            Message = "System is operational",
            StartTime = null,
            EndTime = null
        };

        _mockUnderMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(maintenanceTime);

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("PageNotFound");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public void Index_ShouldReturnViewResult_WhenUnderMaintenance()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = true,
            Message = "System is under maintenance",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2)
        };

        _mockUnderMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(maintenanceTime);

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        // Note: The controller doesn't set the Model, it sets ViewBag.Result instead
    }

    [Fact]
    public void Index_ShouldCallServiceMethod_WhenCalled()
    {
        // Arrange
        var maintenanceTime = new UnderMaintenanceTime
        {
            IsUnderMaintenance = false,
            Message = "System is operational"
        };

        _mockUnderMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(maintenanceTime);

        // Act
        _controller.Index();

        // Assert
        _mockUnderMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldNotThrowException_WhenServiceIsNull()
    {
        // Act & Assert
        // The actual implementation doesn't validate null service in constructor
        var action = () => new UnderMaintenanceController(null!);
        action.Should().NotThrow();
    }
}
