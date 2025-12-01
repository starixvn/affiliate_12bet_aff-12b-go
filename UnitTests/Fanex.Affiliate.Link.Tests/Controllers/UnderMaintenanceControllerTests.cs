using Fanex.Affiliate.Link.Controllers;
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
    public void Constructor_ShouldCreateInstance_WhenServiceProvided()
    {
        // Act
        var controller = new UnderMaintenanceController(_mockUnderMaintenanceService.Object);

        // Assert
        controller.Should().NotBeNull();
    }

    [Fact]
    public void Index_ShouldReturnViewResult_WhenCalled()
    {
        // Arrange
        _mockUnderMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new Fanex.Affiliate.Link.Models.UnderMaintenanceTime { IsUnderMaintenance = false });

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public void Index_ShouldSetViewBagData_WhenServiceReturnsData()
    {
        // Arrange
        _mockUnderMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new Fanex.Affiliate.Link.Models.UnderMaintenanceTime { IsUnderMaintenance = true });

        // Act
        var result = _controller.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        _mockUnderMaintenanceService.Verify(x => x.GetUnderMaintenanceTime(), Times.Once);
    }

    [Fact]
    public void Index_ShouldHandleServiceException()
    {
        // Arrange
        _mockUnderMaintenanceService.Setup(x => x.GetUnderMaintenanceTime())
            .Throws(new System.Exception("Service error"));

        // Act & Assert
        var action = () => _controller.Index();
        action.Should().Throw<System.Exception>().WithMessage("Service error");
    }
}
