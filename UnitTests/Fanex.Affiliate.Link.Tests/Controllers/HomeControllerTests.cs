using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Fanex.Affiliate.Link.Constants;
using Fanex.Affiliate.Link.Controllers;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Affiliate.Link.DTOs;
using Fanex.Affiliate.Link.Services.Interface;
using Fanex.Affiliate.Link.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IClickTrackingEventService> _clickTrackingMock;
    private readonly Mock<IMaintenanceService> _maintenanceMock;
    private readonly Mock<IHttpContextService> _httpContextMock;
    private readonly Mock<ILogService> _logServiceMock;
    private readonly IConfiguration _config;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        var inMemory = new Dictionary<string, string>
        {
            ["AppSettings:RemovePortFromUrl"] = "1",
            ["AppSettings:Redirect_home_pattern"] = "/#language#/home"
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(inMemory!).Build();

        _clickTrackingMock = new Mock<IClickTrackingEventService>(MockBehavior.Strict);
        _maintenanceMock = new Mock<IMaintenanceService>(MockBehavior.Strict);
        _httpContextMock = new Mock<IHttpContextService>(MockBehavior.Strict);
        _logServiceMock = new Mock<ILogService>(MockBehavior.Strict);

        // Setup ILogService methods to avoid LogHelper dependency
        _logServiceMock.Setup(x => x.LogFileError(It.IsAny<Exception>())).Verifiable();
        _logServiceMock.Setup(x => x.LogFileMessage(It.IsAny<string>())).Verifiable();
        _logServiceMock.Setup(x => x.SentryErrorAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _logServiceMock.Setup(x => x.SentryErrorAsync(It.IsAny<Exception>())).Returns(Task.CompletedTask);
        _logServiceMock.Setup(x => x.SentryErrorAsync(It.IsAny<string>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);
        _logServiceMock.Setup(x => x.SentryInfoAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        _httpContextMock.Setup(x => x.GetOriginalProtocol()).Returns("https");
        _httpContextMock.Setup(x => x.GetDeviceName(It.IsAny<string>())).Returns("PC");
        _httpContextMock.Setup(x => x.GetCountry()).Returns("VN");
        _httpContextMock.Setup(x => x.IsMobile()).Returns(false);

        // Default maintenance state: not under maintenance
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        _controller = new HomeController(
            _clickTrackingMock.Object,
            _maintenanceMock.Object,
            _config,
            _httpContextMock.Object,
            _logServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        _controller.HttpContext.Request.Host = new HostString("example.com", 8080);
        _controller.HttpContext.Request.Path = "/landing";
        _controller.HttpContext.Request.Headers["User-Agent"] = "unit-test-agent";
        _controller.HttpContext.Request.Headers["Accept-Language"] = "en";
        _controller.HttpContext.Request.Headers["X-Forwarded-For"] = "127.0.0.1";
    }

    [Fact]
    public void Index_ShouldReturn_ViewResult()
    {
        var result = _controller.Index();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Error_ShouldReturn_ViewResult()
    {
        var result = _controller.Error();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void PageNotFound_ShouldReturn_ViewResult()
    {
        var result = _controller.PageNotFound();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task SyncMaxClickId_ShouldReturn_Ok_WhenSuccess()
    {
        _clickTrackingMock.Setup(x => x.SyncMaxClickId(true)).ReturnsAsync(10L);

        var result = await _controller.SyncMaxClickId();

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(10L);
    }

    [Fact]
    public async Task SyncMaxClickId_ShouldReturn_InternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _clickTrackingMock.Setup(x => x.SyncMaxClickId(true)).ThrowsAsync(exception);
        _logServiceMock.Setup(x => x.SentryErrorAsync(exception)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SyncMaxClickId();

        // Assert
        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(500);
        result.Should().BeOfType<ObjectResult>()
              .Which.Value.Should().Be("An error occurred while syncing max click id: Test exception");
    }

    [Fact]
    public async Task SyncMaxClickId_ShouldReturn_Ok_WhenSyncIsFalse()
    {
        // Arrange
        _clickTrackingMock.Setup(x => x.SyncMaxClickId(false)).ReturnsAsync(5L);

        // Act
        var result = await _controller.SyncMaxClickId(false);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(5L);
    }

    [Fact]
    public void SpecialPageCodeOldLink_ShouldReturn_LinkView_WithOutdatedMessage()
    {
        var result = _controller.SpecialPageCodeOldLink(1, "a", "b") as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Oops! This link is using an outdated format. Please contact the administrator to get it updated.");
    }

    [Fact]
    public void SpecialPageCodeOldLink_ShouldReturn_LinkView_WithLanguage()
    {
        var result = _controller.SpecialPageCodeOldLink(1, "a", "b", "en") as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Oops! This link is using an outdated format. Please contact the administrator to get it updated.");
    }

    [Fact]
    public void OldLink_ShouldReturn_LinkView_WithOutdatedMessage()
    {
        var result = _controller.OldLink(1, "test", "en") as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Oops! This link is using an outdated format. Please contact the administrator to get it updated.");
    }

    [Fact]
    public void OldLink_ShouldReturn_LinkView_WithoutLanguage()
    {
        var result = _controller.OldLink(1, "test") as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Oops! This link is using an outdated format. Please contact the administrator to get it updated.");
    }

    [Fact]
    public async Task SpecialPageCodeLink_ShouldCall_LinkMethod()
    {
        var linkParams = new LinkParams { PageCode = "home", AffCode = "AFF", Lang = "en", Channel = "c" };

        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = true, RedirectLink = "https://member.example/en/home?affCode=AFF" });
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(999L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.SpecialPageCodeLink("home", "page", "AFF", "en", "c");

        result.Should().BeOfType<RedirectResult>();
    }

    [Fact]
    public async Task SpecialPageCodeLink_ShouldHandle_NullParameters()
    {
        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = true, RedirectLink = "https://member.example/en/home?affCode=AFF" });
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(999L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.SpecialPageCodeLink("home", "page", "AFF");

        result.Should().BeOfType<RedirectResult>();
    }

    [Fact]
    public async Task Link_ShouldReturn_View_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("PageCode", "required");

        var result = await _controller.Link(new LinkParams());

        var view = result as ViewResult;
        view.Should().NotBeNull();
        view!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Oops! This link is in an incorrect format. Please reach out to the administrator for assistance.");
    }

    [Fact]
    public async Task Link_ShouldReturn_View_WhenLinkParamsIsNull()
    {
        LinkParams linkParams = null!;

        var result = await _controller.Link(linkParams);

        var view = result as ViewResult;
        view.Should().NotBeNull();
        view!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Oops! This link is in an incorrect format. Please reach out to the administrator for assistance.");
    }

    [Fact]
    public async Task Link_ShouldRedirect_WhenValidateSuccess_NotUnderMaintenance()
    {
        var linkParams = new LinkParams { PageCode = "landing", AffCode = "AFF", Lang = "en", Channel = "c" };

        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = true, RedirectLink = "https://member.example/en/landing?affCode=AFF" });
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(999L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        _controller.HttpContext.Request.QueryString = new QueryString("?x=1");

        var result = await _controller.Link(linkParams);

        var redirect = result as RedirectResult;
        redirect.Should().NotBeNull();
        redirect!.Url.Should().Contain("https://member.example/en/landing?affCode=AFF&clickId=999");
    }

    [Fact]
    public async Task Link_ShouldReturn_LinkView_WhenValidateInvalid()
    {
        var linkParams = new LinkParams { PageCode = "home", AffCode = "AFF", Lang = "en", Channel = "c" };

        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = false, ErrorMessage = "Invalid link" });

        var result = await _controller.Link(linkParams);

        var view = result as ViewResult;
        view.Should().NotBeNull();
        view!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Invalid link");
    }

    [Fact]
    public async Task Link_ShouldReturn_LinkView_WhenExceptionOccurs()
    {
        // Arrange
        var linkParams = new LinkParams { PageCode = "home", AffCode = "AFF", Lang = "en", Channel = "c" };
        var exception = new InvalidOperationException("Test exception");

        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ThrowsAsync(exception);
        _logServiceMock.Setup(x => x.SentryErrorAsync(exception)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Link(linkParams);

        // Assert
        var view = result as ViewResult;
        view.Should().NotBeNull();
        view!.ViewName.Should().Be("Link");
        ((string)_controller.ViewBag.Result).Should().Be("Oops! This link is in an incorrect format. Please reach out to the administrator for assistance.");
    }

    [Fact]
    public async Task ValidateLinkFromCache_ShouldReturn_ValidSpecialDomain()
    {
        // Arrange under maintenance
        _maintenanceMock.Reset();
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = true });

        // Build click event via controller helper
        var initMethod = typeof(HomeController).GetMethod("InitClickTrackingEvent", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var linkParams = new LinkParams { PageCode = "landing", AffCode = "AFF", Lang = "en", Channel = "c" };
        var clickEvent = (ClickTrackingEvent)initMethod.Invoke(_controller, new object[] { linkParams })!;

        // Master data that matches computed AffDomain (example.com/landing)
        var master = new MasterInfoValidation
        {
            SpecialDomainMappings = new[]
            {
                new SpecialDomainMapping { AffCode = "AFF", AffDomain = clickEvent.AffDomain, RedirectDomain = "special.example", IsSSL = true, IsMobile = true }
            },
            LanguageReferences = new[]
            {
                new LanguageReference { LanguageCode = clickEvent.LanguageCode, LanguageLink = "en" }
            },
            AffiliateDeals = new[] { new AffiliateDeal { AffCode = "AFF", DealName = "MarketingGroup", DealId = 1 } },
            DomainMappings = Array.Empty<DomainMapping>()
        };

        _clickTrackingMock.Setup(x => x.GetMasterDataForValidateClicked()).ReturnsAsync(master);

        // Act: call private async ValidateLinkFromCache
        var method = typeof(HomeController).GetMethod("ValidateLinkFromCache", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var task = (Task<ValidateResult>)method.Invoke(_controller, new object[] { clickEvent })!;
        var result = await task;

        // Assert
        result.IsValid.Should().BeTrue();
        result.RedirectLink.Should().Be("https://special.example/en/landing?affCode=AFF");
    }

    [Fact]
    public void GetSupportedPageCodeOnV2_Should_Map_KnownAndUnknown()
    {
        var method = typeof(HomeController).GetMethod("GetSupportedPageCodeOnV2", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, new object[] { "signup" }).Should().Be(LandingPage.JOIN_PAGE_CODE);
        method.Invoke(null, new object[] { "unknown" }).Should().Be("unknown");
    }

    [Fact]
    public void GetSupportedLanguageCodeOnMemberSiteV2_Should_Map_Languages()
    {
        var method = typeof(HomeController).GetMethod("GetSupportedLanguageCodeOnMemberSiteV2", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, new object[] { "hien" }).Should().Be("en");
        method.Invoke(null, new object[] { "id" }).Should().Be("id");
    }

    [Fact]
    public void GetMappingRemovedLanguageCode_Should_MapRemoved()
    {
        var method = typeof(HomeController).GetMethod("GetMappingRemovedLanguageCode", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, new object[] { "bn" }).Should().Be("bd");
    }

    [Fact]
    public async Task ValidateLinkFromCache_ShouldReturn_ValidDomainMapping()
    {
        // Arrange under maintenance
        _maintenanceMock.Reset();
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = true });

        // Build click event via controller helper
        var initMethod = typeof(HomeController).GetMethod("InitClickTrackingEvent", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var linkParams = new LinkParams { PageCode = "landing", AffCode = "AFF", Lang = "en", Channel = "c" };
        var clickEvent = (ClickTrackingEvent)initMethod.Invoke(_controller, new object[] { linkParams })!;

        var master = new MasterInfoValidation
        {
            // No special domain so it should fallback to DomainMappings
            SpecialDomainMappings = Array.Empty<SpecialDomainMapping>(),
            DomainMappings = new[]
            {
                new DomainMapping { AffDomain = "example.com", LanguageCode = clickEvent.LanguageCode, LanguageLink = "en", RedirectDomain = "member.example", IsSSL = true, IsExcluded = true }
            },
            LanguageReferences = Array.Empty<LanguageReference>(),
            AffiliateDeals = new[] { new AffiliateDeal { AffCode = "AFF", DealName = "MarketingGroup", DealId = 1 } },
            LandingPages = new[]
            {
                new LandingPageSetting { PageCode = "landing", IsDefault = true, IsExcluded = true }
            }
        };

        _clickTrackingMock.Setup(x => x.GetMasterDataForValidateClicked()).ReturnsAsync(master);

        var validateFromCacheMethod = typeof(HomeController).GetMethod("ValidateLinkFromCache", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var task = (Task<ValidateResult>)validateFromCacheMethod.Invoke(_controller, new object[] { clickEvent })!;
        var result = await task;

        result.IsValid.Should().BeTrue();
        result.RedirectLink.Should().Be("https://member.example/en/landing?affCode=AFF");
    }

    [Fact]
    public async Task Link_ShouldMapMarketingPageCode_WhenNonMarketingAffiliate()
    {
        var linkParams = new LinkParams { PageCode = "promo", AffCode = "AFF", Lang = "en", Channel = "c" };
        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = true, RedirectLink = "https://member.example/en/promo?affCode=AFF" });
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(2L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>())).Returns(Task.CompletedTask);

        var result = await _controller.Link(linkParams);
        var redirect = result as RedirectResult;
        redirect.Should().NotBeNull();
        redirect!.Url.Should().Contain("clickId=2"); // Check that clickId is appended
    }

    [Fact]
    public void GetHostFromUri_Should_Return_Uri_WhenValid()
    {
        var method = typeof(HomeController).GetMethod("GetHostFromUri", BindingFlags.NonPublic | BindingFlags.Static)!;
        var uri = (Uri)method.Invoke(null, new object[] { "https://abc.com/x" })!;
        uri.Host.Should().Be("abc.com");
    }

    [Fact]
    public void GetHostFromUri_Should_Return_Null_WhenInvalid()
    {
        var method = typeof(HomeController).GetMethod("GetHostFromUri", BindingFlags.NonPublic | BindingFlags.Static)!;
        var uri = method.Invoke(null, new object[] { "invalid-uri" });
        uri.Should().BeNull();
    }

    [Fact]
    public void HandleRedirectLink_Should_Adjust_BengaliLanguage_And_PageCode()
    {
        var method = typeof(HomeController).GetMethod("HandleRedirectLink", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var redirect = "https://example.com/bn/join?affCode=abc";
        var result = (string)method.Invoke(_controller, new object[] { redirect, "join", "bd" })!;
        result.Should().Contain("/bd/");
    }

    [Fact]
    public void GetFullAffDomain_Should_Remove_Port_And_Path()
    {
        var method = typeof(HomeController).GetMethod("GetFullAffDomain", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_controller, new object[] { "example.com:8080/landing", "landing" })!;
        result.Should().Be("example.com");
        
        // Debug: Let's see what the actual result is
        Console.WriteLine($"GetFullAffDomain result: '{result}'");
    }

    [Fact]
    public void InitClickTrackingEvent_Should_Use_DeviceType_From_IsMobile()
    {
        _httpContextMock.Reset();
        _httpContextMock.Setup(x => x.GetOriginalProtocol()).Returns("https");
        _httpContextMock.Setup(x => x.GetDeviceName(It.IsAny<string>())).Returns("Mobile");
        _httpContextMock.Setup(x => x.GetCountry()).Returns("VN");
        _httpContextMock.Setup(x => x.IsMobile()).Returns(true);

        var controller = new HomeController(
            _clickTrackingMock.Object,
            _maintenanceMock.Object,
            _config,
            _httpContextMock.Object,
            _logServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Host = new HostString("example.com");
        controller.HttpContext.Request.Path = "/landing";
        controller.HttpContext.Request.Headers["User-Agent"] = "ua";
        controller.HttpContext.Request.Headers["Accept-Language"] = "en";

        var method = typeof(HomeController).GetMethod("InitClickTrackingEvent", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var evt = (ClickTrackingEvent)method.Invoke(controller, new object[] { new LinkParams { PageCode = "home", AffCode = "A", Lang = "en", Channel = "c" } })!;
        evt.DeviceType.Should().Be(1);
    }

    [Fact]
    public void GetDeviceType_Should_Return_One_When_Mobile()
    {
        _httpContextMock.Reset();
        _httpContextMock.Setup(x => x.IsMobile()).Returns(true);

        var method = typeof(HomeController).GetMethod("GetDeviceType", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (byte)method.Invoke(_controller, new object[] { })!;
        result.Should().Be(1);
    }

    [Fact]
    public void GetDeviceType_Should_Return_Zero_When_Desktop()
    {
        _httpContextMock.Reset();
        _httpContextMock.Setup(x => x.IsMobile()).Returns(false);

        var method = typeof(HomeController).GetMethod("GetDeviceType", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (byte)method.Invoke(_controller, new object[] { })!;
        result.Should().Be(0);
    }

    [Fact]
    public void GetAffMediaLinkWithoutProtocol_Should_Return_HostAndPath()
    {
        var method = typeof(HomeController).GetMethod("GetAffMediaLinkWithoutProtocol", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_controller, new object[] { })!;
        result.Should().Be("example.com:8080/landing");
    }

    [Fact]
    public void HandleRedirectLink_Should_Handle_Null_RedirectLink()
    {
        var method = typeof(HomeController).GetMethod("HandleRedirectLink", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_controller, new object[] { null, "home", "en" })!;
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void HandleRedirectLink_Should_Handle_Empty_Parameters()
    {
        var method = typeof(HomeController).GetMethod("HandleRedirectLink", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_controller, new object[] { "https://example.com", "", "" })!;
        result.Should().Be("https://example.com");
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        _controller.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenClickTrackingServiceIsNull()
    {
        var action = () => new HomeController(
            null!,
            _maintenanceMock.Object,
            _config,
            _httpContextMock.Object,
            _logServiceMock.Object);

        action.Should().Throw<ArgumentNullException>()
              .WithParameterName("trackingSystemService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenMaintenanceServiceIsNull()
    {
        var action = () => new HomeController(
            _clickTrackingMock.Object,
            null!,
            _config,
            _httpContextMock.Object,
            _logServiceMock.Object);

        action.Should().Throw<ArgumentNullException>()
              .WithParameterName("maintenanceService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        var action = () => new HomeController(
            _clickTrackingMock.Object,
            _maintenanceMock.Object,
            null!,
            _httpContextMock.Object,
            _logServiceMock.Object);

        action.Should().Throw<ArgumentNullException>()
              .WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpContextServiceIsNull()
    {
        var action = () => new HomeController(
            _clickTrackingMock.Object,
            _maintenanceMock.Object,
            _config,
            null!,
            _logServiceMock.Object);

        action.Should().Throw<ArgumentNullException>()
              .WithParameterName("httpContextService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLogServiceIsNull()
    {
        var action = () => new HomeController(
            _clickTrackingMock.Object,
            _maintenanceMock.Object,
            _config,
            _httpContextMock.Object,
            null!);

        action.Should().Throw<ArgumentNullException>()
              .WithParameterName("logService");
    }

    [Fact]
    public async Task Link_ShouldHandle_UnderMaintenanceMode()
    {
        // Arrange
        var linkParams = new LinkParams { PageCode = "landing", AffCode = "AFF", Lang = "en", Channel = "c" };

        _maintenanceMock.Reset();
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = true });

        var master = new MasterInfoValidation
        {
            SpecialDomainMappings = Array.Empty<SpecialDomainMapping>(),
            DomainMappings = new[]
            {
                new DomainMapping { AffDomain = "example.com", LanguageCode = "en", LanguageLink = "en", RedirectDomain = "member.example", IsSSL = true }
            },
            LanguageReferences = new[] { new LanguageReference { LanguageCode = "en", LanguageLink = "en" } },
            AffiliateDeals = new[] { new AffiliateDeal { AffCode = "AFF", DealName = "MarketingGroup", DealId = 1 } },
            LandingPages = new[]
            {
                new LandingPageSetting { PageCode = "landing", IsDefault = true }
            }
        };

        _clickTrackingMock.Setup(x => x.GetMasterDataForValidateClicked()).ReturnsAsync(master);
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(999L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Link(linkParams);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Link_ShouldHandle_DatabaseException_AndFallbackToCache()
    {
        // Arrange
        var linkParams = new LinkParams { PageCode = "landing", AffCode = "AFF", Lang = "en", Channel = "c" };
        var exception = new InvalidOperationException("Database error");

        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ThrowsAsync(exception);
        _logServiceMock.Setup(x => x.SentryErrorAsync(exception)).Returns(Task.CompletedTask);

        var master = new MasterInfoValidation
        {
            SpecialDomainMappings = Array.Empty<SpecialDomainMapping>(),
            DomainMappings = new[]
            {
                new DomainMapping { AffDomain = "example.com", LanguageCode = "en", LanguageLink = "en", RedirectDomain = "member.example", IsSSL = true }
            },
            LanguageReferences = new[] { new LanguageReference { LanguageCode = "en", LanguageLink = "en" } },
            AffiliateDeals = new[] { new AffiliateDeal { AffCode = "AFF", DealName = "MarketingGroup", DealId = 1 } },
            LandingPages = new[]
            {
                new LandingPageSetting { PageCode = "landing", IsDefault = true }
            }
        };

        _clickTrackingMock.Setup(x => x.GetMasterDataForValidateClicked()).ReturnsAsync(master);
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(999L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Link(linkParams);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Link_ShouldHandle_CacheException_AndFallbackToDatabase()
    {
        // Arrange
        var linkParams = new LinkParams { PageCode = "home", AffCode = "AFF", Lang = "en", Channel = "c" };

        _maintenanceMock.Reset();
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = true });

        var exception = new InvalidOperationException("Cache error");
        _clickTrackingMock.Setup(x => x.GetMasterDataForValidateClicked()).ThrowsAsync(exception);
        _logServiceMock.Setup(x => x.SentryErrorAsync(exception)).Returns(Task.CompletedTask);

        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = true, RedirectLink = "https://member.example/en/home?affCode=AFF" });
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(999L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Link(linkParams);

        // Assert
        result.Should().BeOfType<RedirectResult>();
    }

    [Fact]
    public void GetSupportedPageCodeOnV2_Should_Handle_NullAndEmpty()
    {
        var method = typeof(HomeController).GetMethod("GetSupportedPageCodeOnV2", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, new object[] { "" }).Should().Be("");
        method.Invoke(null, new object[] { (string)null! }).Should().Be("");
    }

    [Fact]
    public void GetSupportedLanguageCodeOnMemberSiteV2_Should_Handle_NullAndEmpty()
    {
        var method = typeof(HomeController).GetMethod("GetSupportedLanguageCodeOnMemberSiteV2", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, new object[] { "" }).Should().Be("");
        method.Invoke(null, new object[] { (string)null! }).Should().Be("");
    }

    [Fact]
    public void GetMappingRemovedLanguageCode_Should_Handle_NullAndEmpty()
    {
        var method = typeof(HomeController).GetMethod("GetMappingRemovedLanguageCode", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, new object[] { "" }).Should().Be("");
        method.Invoke(null, new object[] { (string)null! }).Should().Be("");
    }

    [Fact]
    public void GetHostFromUri_Should_Return_Null_WhenEmpty()
    {
        var method = typeof(HomeController).GetMethod("GetHostFromUri", BindingFlags.NonPublic | BindingFlags.Static)!;
        var uri = method.Invoke(null, new object[] { "" });
        uri.Should().BeNull();
    }

    [Fact]
    public void HandleRedirectLink_Should_Handle_CustomRedirectPattern()
    {
        var method = typeof(HomeController).GetMethod("HandleRedirectLink", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var redirect = "https://example.com/en/home?affCode=abc";

        var result = (string)method.Invoke(_controller, new object[] { redirect, "home", "en" })!;

        result.Should().Contain("/en/home");
    }

    [Fact]
    public void GetFullAffDomain_Should_Handle_EmptyParameters()
    {
        var method = typeof(HomeController).GetMethod("GetFullAffDomain", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(_controller, new object[] { "", "landing" }).Should().Be("");
        method.Invoke(_controller, new object[] { "example.com", "" }).Should().Be("");
        method.Invoke(_controller, new object[] { "", "" }).Should().Be("");
    }

    [Fact]
    public void InitClickTrackingEvent_Should_Handle_RefererHeader()
    {
        // Arrange
        _controller.HttpContext.Request.Headers["Referer"] = "https://google.com/search";

        var method = typeof(HomeController).GetMethod("InitClickTrackingEvent", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        var evt = (ClickTrackingEvent)method.Invoke(_controller, new object[] { new LinkParams { PageCode = "home", AffCode = "A", Lang = "en", Channel = "c" } })!;

        // Assert
        evt.UrlReferrer.Should().Be("https://google.com/search");
        evt.DomainReferrer.Should().Be("google.com");
    }

    [Fact]
    public void InitClickTrackingEvent_Should_Handle_RefQueryParameter()
    {
        // Arrange
        _controller.HttpContext.Request.QueryString = new QueryString("?ref=https://facebook.com/post");

        var method = typeof(HomeController).GetMethod("InitClickTrackingEvent", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        var evt = (ClickTrackingEvent)method.Invoke(_controller, new object[] { new LinkParams { PageCode = "home", AffCode = "A", Lang = "en", Channel = "c" } })!;

        // Assert
        evt.UrlReferrer.Should().Be("https://facebook.com/post");
        evt.DomainReferrer.Should().NotBeEmpty();
    }

    [Fact]
    public void IsUnderMaintenance_Should_Return_True_WhenUnderMaintenance()
    {
        // Arrange
        _maintenanceMock.Reset();
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = true });

        var method = typeof(HomeController).GetMethod("IsUnderMaintenance", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        var result = (bool)method.Invoke(_controller, new object[] { })!;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUnderMaintenance_Should_Return_False_WhenNotUnderMaintenance()
    {
        // Arrange
        _maintenanceMock.Reset();
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        var method = typeof(HomeController).GetMethod("IsUnderMaintenance", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        var result = (bool)method.Invoke(_controller, new object[] { })!;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsUnderMaintenance_Should_Return_False_WhenMaintenanceTimeIsNull()
    {
        // Arrange
        _maintenanceMock.Reset();
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns((ONEbook.UM.Models.UnderMaintenanceTime)null!);

        var method = typeof(HomeController).GetMethod("IsUnderMaintenance", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        var result = (bool)method.Invoke(_controller, new object[] { })!;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateLinkDomain_Should_Return_Invalid_WhenClickTrackingEventIsNull()
    {
        var method = typeof(HomeController).GetMethod("ValidateLinkDomain", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var master = new MasterInfoValidation();

        var result = (ValidateResult)method.Invoke(_controller, new object[] { null!, master })!;

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Oops! This link does not exist. Please reach out to the administrator if you need help.");
    }

    [Fact]
    public void ValidateLinkDomain_Should_Return_Invalid_WhenMasterInfoValidationIsNull()
    {
        var method = typeof(HomeController).GetMethod("ValidateLinkDomain", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var clickEvent = new ClickTrackingEvent
        {
            AffCode = "AFF",
            AffDomain = "example.com",
            PageCode = "home",
            LanguageCode = "en",
            ChannelCode = "c",
            IP = "127.0.0.1",
            Browser = "Test",
            UrlReferrer = "",
            DomainReferrer = "",
            OS = "Test",
            Device = "Test",
            BrowserVersion = "1.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en",
            UserAgent = "Test",
            Country = "VN",
            RedirectLink = "",
            AffMediaLink = "https://example.com"
        };

        var result = (ValidateResult)method.Invoke(_controller, new object[] { clickEvent, null! })!;

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Oops! This link does not exist. Please reach out to the administrator if you need help.");
    }

    [Fact]
    public void ValidateLinkSpecialDomain_Should_Return_Invalid_WhenDomainNotFound()
    {
        var method = typeof(HomeController).GetMethod("ValidateLinkSpecialDomain", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var clickEvent = new ClickTrackingEvent 
        { 
            AffCode = "AFF", 
            AffDomain = "example.com",
            PageCode = "home",
            LanguageCode = "en",
            ChannelCode = "c",
            IP = "127.0.0.1",
            Browser = "Test",
            UrlReferrer = "",
            DomainReferrer = "",
            OS = "Test",
            Device = "Test",
            BrowserVersion = "1.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en",
            UserAgent = "Test",
            Country = "VN",
            RedirectLink = "",
            AffMediaLink = "https://example.com"
        };
        var master = new MasterInfoValidation
        {
            SpecialDomainMappings = Array.Empty<SpecialDomainMapping>(),
            LanguageReferences = Array.Empty<LanguageReference>()
        };

        var result = (ValidateResult)method.Invoke(_controller, new object[] { clickEvent, master })!;

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Oops! This link does not exist. Please reach out to the administrator if you need help.");
    }

    [Fact]
    public void ValidateLinkSpecialDomain_Should_Return_Valid_WhenDomainFound()
    {
        var method = typeof(HomeController).GetMethod("ValidateLinkSpecialDomain", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var clickEvent = new ClickTrackingEvent 
        { 
            AffCode = "AFF", 
            AffDomain = "example.com", 
            PageCode = "home",
            LanguageCode = "en",
            ChannelCode = "c",
            IP = "127.0.0.1",
            Browser = "Test",
            UrlReferrer = "",
            DomainReferrer = "",
            OS = "Test",
            Device = "Test",
            BrowserVersion = "1.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en",
            UserAgent = "Test",
            Country = "VN",
            RedirectLink = "",
            AffMediaLink = "https://example.com"
        };
        var master = new MasterInfoValidation
        {
            SpecialDomainMappings = new[]
            {
                new SpecialDomainMapping { AffCode = "AFF", AffDomain = "example.com", RedirectDomain = "special.example", IsSSL = true, IsMobile = false }
            },
            LanguageReferences = new[]
            {
                new LanguageReference { LanguageCode = "en", LanguageLink = "en" }
            }
        };

        var result = (ValidateResult)method.Invoke(_controller, new object[] { clickEvent, master })!;

        result.IsValid.Should().BeTrue();
        result.RedirectLink.Should().Be("https://special.example/en/home?affCode=AFF");
    }
}


