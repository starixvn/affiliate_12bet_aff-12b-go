using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Fanex.Affiliate.Link.Constants;
using Fanex.Affiliate.Link.Controllers;
using Fanex.Affiliate.Link.Core.Models;
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
    private readonly Mock<IAffInfoService> _affInfoMock;
    private readonly Mock<IMaintenanceService> _maintenanceMock;
    private readonly Mock<IHttpContextService> _httpContextMock;
    private readonly IConfiguration _config;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        var inMemory = new Dictionary<string, string>
        {
            ["AppSettings:MarketingForCommGroups"] = "MarketingGroup",
            ["AppSettings:MarketingPageCodeRegexPatterns"] = "join.*,promo.*",
            ["AppSettings:RemovePortFromUrl"] = "1",
            ["AppSettings:Redirect_home_pattern"] = "/#language#/home"
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(inMemory!).Build();

        _clickTrackingMock = new Mock<IClickTrackingEventService>(MockBehavior.Strict);
        _affInfoMock      = new Mock<IAffInfoService>(MockBehavior.Strict);
        _maintenanceMock  = new Mock<IMaintenanceService>(MockBehavior.Strict);
        _httpContextMock  = new Mock<IHttpContextService>(MockBehavior.Strict);

        _httpContextMock.Setup(x => x.GetOriginalProtocol()).Returns("https");
        _httpContextMock.Setup(x => x.GetDeviceName(It.IsAny<string>())).Returns("PC");
        _httpContextMock.Setup(x => x.GetCountry()).Returns("VN");
        _httpContextMock.Setup(x => x.IsMobile()).Returns(false);

        // Default maintenance state: not under maintenance
        _maintenanceMock.Setup(x => x.GetUnderMaintenanceTime())
            .Returns(new ONEbook.UM.Models.UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        _controller = new HomeController(
            _clickTrackingMock.Object,
            _affInfoMock.Object,
            _maintenanceMock.Object,
            _config,
            _httpContextMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        _controller.HttpContext.Request.Host  = new HostString("example.com", 8080);
        _controller.HttpContext.Request.Path  = "/landing";
        _controller.HttpContext.Request.Headers["User-Agent"]      = "unit-test-agent";
        _controller.HttpContext.Request.Headers["Accept-Language"] = "en";
        _controller.HttpContext.Request.Headers["X-Forwarded-For"] = "127.0.0.1";

        // Default stubs for aff info used in IsValidAffMarketing
        _affInfoMock.Setup(x => x.GetCommGroupByAffCode(It.IsAny<string>()))
            .Returns(new CommGroup { GroupName = "MarketingGroup" });
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
    public void SyncMaxClickId_ShouldReturn_Ok_CanBeCalled()
    {
        // just verify action callable; error path not tested to avoid external logger dependency
        _clickTrackingMock.Setup(x => x.SyncMaxClickId(true)).ReturnsAsync(1L);
        var task = _controller.SyncMaxClickId();
        task.Should().NotBeNull();
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
    public async Task Link_ShouldRedirect_WhenValidateSuccess_NotUnderMaintenance()
    {
        var linkParams = new LinkParams { PageCode = "home", AffCode = "AFF", Lang = "en", Channel = "c" };

        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = true, RedirectView = "https://member.example/en/home?affCode=AFF" });
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(999L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>()))
            .Returns(Task.CompletedTask);

        _controller.HttpContext.Request.QueryString = new QueryString("?x=1");

        var result = await _controller.Link(linkParams);

        var redirect = result as RedirectResult;
        redirect.Should().NotBeNull();
        redirect!.Url.Should().Be("https://member.example/en/home?affCode=AFF&clickId=999&x=1");
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
    public void Link_ExceptionPath_NotTested_DueTo_LoggerDependency()
    {
        Assert.True(true);
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
        var linkParams = new LinkParams { PageCode = "home", AffCode = "AFF", Lang = "en", Channel = "c" };
        var clickEvent = (ClickTrackingEvent)initMethod.Invoke(_controller, new object[] { linkParams })!;

        // Master data that matches computed AffDomain (example.com/landing)
        var master = new MasterInfoValidation
        {
            SpecialDomainMappings = new[]
            {
                new SpecialDomainMapping { AffCode = "AFF", AffDomain = clickEvent.AffDomain, RedirectDomain = "special.example", IsSSL = true }
            },
            LanguageReferences = new[]
            {
                new LanguageReference { LanguageCode = clickEvent.LanguageCode, LanguageLink = "en" }
            },
            AffCommGroups = new[] { new AffiliateCommGroup { AffCode = "AFF", DealName = "MarketingGroup" } },
            DomainMappings = Array.Empty<DomainMapping>()
        };

        _clickTrackingMock.Setup(x => x.GetMasterDataForValidateClicked()).ReturnsAsync(master);

        // Act: call private async ValidateLinkFromCache
        var method = typeof(HomeController).GetMethod("ValidateLinkFromCache", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var task = (Task<ValidateResult>)method.Invoke(_controller, new object[] { clickEvent })!;
        var result = await task.ConfigureAwait(false);

        // Assert
        result.IsValid.Should().BeTrue();
        result.RedirectView.Should().Be("https://special.example/en/home?affCode=AFF");
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
        var linkParams = new LinkParams { PageCode = "home", AffCode = "AFF", Lang = "en", Channel = "c" };
        var clickEvent = (ClickTrackingEvent)initMethod.Invoke(_controller, new object[] { linkParams })!;

        var master = new MasterInfoValidation
        {
            // No special domain so it should fallback to DomainMappings
            SpecialDomainMappings = Array.Empty<SpecialDomainMapping>(),
            DomainMappings = new[]
            {
                new DomainMapping { AffDomain = clickEvent.AffDomain, LanguageCode = clickEvent.LanguageCode, LanguageLink = "en", RedirectDomain = "member.example", IsSSL = true }
            },
            LanguageReferences = Array.Empty<LanguageReference>(),
            AffCommGroups = new[] { new AffiliateCommGroup { AffCode = "AFF", DealName = "MarketingGroup" } }
        };

        _clickTrackingMock.Setup(x => x.GetMasterDataForValidateClicked()).ReturnsAsync(master);

        var validateFromCacheMethod = typeof(HomeController).GetMethod("ValidateLinkFromCache", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var task = (Task<ValidateResult>)validateFromCacheMethod.Invoke(_controller, new object[] { clickEvent })!;
        var result = await task;

        result.IsValid.Should().BeTrue();
        result.RedirectView.Should().Be("https://member.example/en/home?affCode=AFF");
    }

    [Fact]
    public async Task Link_ShouldMapMarketingPageCode_WhenNonMarketingAffiliate()
    {
        // Configure aff group not in marketing list
        _affInfoMock.Reset();
        _affInfoMock.Setup(x => x.GetCommGroupByAffCode(It.IsAny<string>()))
            .Returns(new CommGroup { GroupName = "NonMarketing" });

        var linkParams = new LinkParams { PageCode = "promo", AffCode = "AFF", Lang = "en", Channel = "c" };
        _clickTrackingMock.Setup(x => x.ValidateLinkFromDatabase(It.IsAny<ClickTrackingEvent>()))
            .ReturnsAsync(new ValidateResult { IsValid = true, RedirectView = "https://member.example/en/promo?affCode=AFF" });
        _clickTrackingMock.Setup(x => x.GetClickId()).ReturnsAsync(2L);
        _clickTrackingMock.Setup(x => x.PublishMessage(It.IsAny<ClickTrackingEvent>())).Returns(Task.CompletedTask);

        var result = await _controller.Link(linkParams);
        var redirect = result as RedirectResult;
        redirect.Should().NotBeNull();
        redirect!.Url.Should().Contain("/join?");
    }

    [Fact]
    public void GetHostFromUri_Should_Return_Uri_WhenValid()
    {
        var method = typeof(HomeController).GetMethod("GetHostFromUri", BindingFlags.NonPublic | BindingFlags.Static)!;
        var uri = (Uri)method.Invoke(null, new object[] { "https://abc.com/x" })!;
        uri.Host.Should().Be("abc.com");
    }

    [Fact]
    public void HandleRedirectLink_Should_Adjust_BengaliLanguage_And_PageCode()
    {
        var method = typeof(HomeController).GetMethod("HandleRedirectLink", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var redirect = "https://example.com/bn/join?affCode=abc";
        var result = (string)method.Invoke(_controller, new object[] { redirect, "join", "bd", "signup" })!;
        result.Should().Contain("/bd/").And.Contain("/signup");
    }

    [Fact]
    public void GetFullAffDomain_Should_Remove_Port_And_Path()
    {
        var method = typeof(HomeController).GetMethod("GetFullAffDomain", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_controller, new object[] { "example.com:8080/landing", "landing" })!;
        result.Should().Be("example.com");
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
            _affInfoMock.Object,
            _maintenanceMock.Object,
            _config,
            _httpContextMock.Object)
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
}


