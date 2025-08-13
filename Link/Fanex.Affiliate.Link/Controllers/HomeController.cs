namespace Fanex.Affiliate.Link.Controllers
{
    using Fanex.Affiliate.Link.Constants;
    using Fanex.Affiliate.Link.Core.Models;
    using Fanex.Affiliate.Link.DTOs;
    using Fanex.Affiliate.Link.Extensions;
    using Fanex.Affiliate.Link.Models;
    using Helpers;
    using Microsoft.AspNetCore.Mvc;
    using ONEbook.UM.Services;
    using Services.Interface;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>
    /// Home controller for affiliate link.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly string[] marketingForCommGroups;
        private readonly string[] marketingPageCodeRegexPatterns;
        private readonly bool isRemovePortFromUrl;

        private readonly IClickTrackingEventService trackingSystemService;
        private readonly IAffInfoService affInfoService;
        private readonly IConfiguration configuration;
        private readonly IMaintenanceService maintenanceService;
        private readonly IHttpContextService httpContextService;
        private const string LinkIsInvalidMessage = "Oops! This link does not exist. Please reach out to the administrator if you need help.";
        private const string LinkIsIncorrectMessage = "Oops! This link is in an incorrect format. Please reach out to the administrator for assistance.";
        private const string LinkIsOutdatedMessage = "Oops! This link is using an outdated format. Please contact the administrator to get it updated.";
        
        // HTTP Status Codes
        private const int HttpStatusInternalServerError = 500;

        public HomeController(
            IClickTrackingEventService trackingSystemService,
            IAffInfoService affInfoService,
            IMaintenanceService maintenanceService,
            IConfiguration configuration,
            IHttpContextService httpContextService)
        {
            this.trackingSystemService = trackingSystemService;
            this.affInfoService = affInfoService;
            this.configuration = configuration;
            this.maintenanceService = maintenanceService;
            this.httpContextService = httpContextService;
            marketingForCommGroups = (this.configuration["AppSettings:MarketingForCommGroups"] ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            marketingPageCodeRegexPatterns = (this.configuration["AppSettings:MarketingPageCodeRegexPatterns"] ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            isRemovePortFromUrl = this.configuration["AppSettings:RemovePortFromUrl"].MatchWith("1");
        }

        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Error() => View();

        [HttpGet]
        public IActionResult PageNotFound() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncMaxClickId(bool isSync = true)
        {
            try
            {
                var result = await trackingSystemService.SyncMaxClickId(isSync);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex);
                return StatusCode(HttpStatusInternalServerError, $"An error occurred while syncing max click id: {ex.Message}");
            }
        }

        public IActionResult SpecialPageCodeOldLink(int affId, string pageCodePart1, string pageCodePart2, string lang = null)
        {
            return OldLink(affId, $"{pageCodePart1}/{pageCodePart2}", lang);
        }

        public IActionResult OldLink(int affId, string pageCode, string lang = null)
        {
            ViewBag.Result = LinkIsOutdatedMessage;
            return View("Link");
        }

        public async Task<IActionResult> SpecialPageCodeLink(string pageCodePart1, string pageCodePart2, string affCode, string lang = null, string channel = null)
        {
            var linkParams = new LinkParams
            {
                PageCode = $"{pageCodePart1}/{pageCodePart2}",
                AffCode = affCode,
                Lang = lang,
                Channel = channel
            };

            return await Link(linkParams).ConfigureAwait(false);
        }

        public async Task<IActionResult> Link(LinkParams linkParams)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Result = LinkIsIncorrectMessage;
                return View("Link");
            }

            try
            {
                var clickTrackingEvent = InitClickTrackingEvent(linkParams);
                var validateResult = await ValidateLink(clickTrackingEvent).ConfigureAwait(false);
                if (validateResult.IsValid)
                {
                    clickTrackingEvent.RedirectLink = validateResult.RedirectView;
                    return await ProcessLink(clickTrackingEvent).ConfigureAwait(false);
                }

                ViewBag.Result = validateResult.ErrorMessage;
                return View("Link");
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex);
                ViewBag.Result = LinkIsIncorrectMessage;
                return View("Link");
            }
        }

        private ClickTrackingEvent InitClickTrackingEvent(LinkParams linkParams)
        {
            var affMediaLinkWithoutProtocol = GetAffMediaLinkWithoutProtocol();
            var affDomain = GetFullAffDomain(affMediaLinkWithoutProtocol, linkParams.PageCode);

            string urlReferrer = Request.Headers["Referer"].ToString();
            string domainReferrer = !string.IsNullOrEmpty(urlReferrer) ? 
                GetHostFromUri(urlReferrer)?.Host ?? string.Empty : string.Empty;

            if (string.IsNullOrEmpty(urlReferrer) && !string.IsNullOrEmpty(Request.Query["ref"].ToString()))
            {
                urlReferrer = Request.Query["ref"].ToString();
                domainReferrer = Common.GetDomainReferrer(urlReferrer) ?? string.Empty;
            }

            string pageCodeOnV2 = GetSupportedPageCodeOnV2(linkParams.PageCode);
            string mappingRemovedLanguage = GetMappingRemovedLanguageCode(linkParams.Lang);
            string userAgent = Request.Headers["User-Agent"].ToString();
            string acceptLanguage = Request.Headers["Accept-Language"].ToString();

            var clickTrackingEvent = new ClickTrackingEvent
            {
                PageCode = pageCodeOnV2,
                AffCode = linkParams.AffCode,
                LanguageCode = mappingRemovedLanguage,
                ChannelCode = linkParams.Channel,
                AffDomain = affDomain,
                AffMediaLink = $"{httpContextService.GetOriginalProtocol()}://{affMediaLinkWithoutProtocol}",
                IP = Request.GetClientIP(),
                Browser = Common.GetBrowserName(userAgent),
                UrlReferrer = urlReferrer,
                DomainReferrer = domainReferrer,
                OS = Common.GetOsName(userAgent),
                Device = httpContextService.GetDeviceName(userAgent),
                BrowserVersion = Common.GetBrowserVersion(userAgent),
                BrowserResolution = string.Empty, // Will be set later if needed
                BrowserLanguage = acceptLanguage,
                UserAgent = userAgent,
                Country = httpContextService.GetCountry(),
                DeviceType = GetDeviceType(),
                RedirectLink = string.Empty // Will be set later in ProcessLink method
            };

            return clickTrackingEvent;
        }

        private static Uri GetHostFromUri(string uriString)
        {
            try
            {
                var uri = new Uri(uriString);
                return uri;
            }
            catch
            {
                return null;
            }
        }

        private async Task<IActionResult> ProcessLink(ClickTrackingEvent clickTrackingEvent)
        {
            var clickId = await trackingSystemService.GetClickId().ConfigureAwait(false);
            clickTrackingEvent.ClickId = clickId;
            clickTrackingEvent.UpdatedTime = DateTimeHelper.GmtMinus4Now;
            clickTrackingEvent.RedirectLink = $"{clickTrackingEvent.RedirectLink}&clickId={clickId}";

            await trackingSystemService.PublishMessage(clickTrackingEvent).ConfigureAwait(false);

            var forwardQueryString = Request.QueryString.Value.ReplaceStr("?", "&");
            var redirectLink = $"{clickTrackingEvent.RedirectLink}{forwardQueryString}";
            return Redirect(redirectLink);
        }

        private bool IsUnderMaintenance()
        {
            var maintanceTime = maintenanceService.GetUnderMaintenanceTime();
            return maintanceTime != null && maintanceTime.IsUnderMaintenanceTime;
        }

        private async Task<ValidateResult> ValidateLink(ClickTrackingEvent clickTrackingEvent)
        {
            ValidateResult validateResult;
            if (IsUnderMaintenance())
            {
                validateResult = await ValidateLinkFromCache(clickTrackingEvent).ConfigureAwait(false);
            }
            else
            {
                validateResult = await trackingSystemService.ValidateLinkFromDatabase(clickTrackingEvent).ConfigureAwait(false);
                if (validateResult.IsValid)
                {
                    validateResult.IsAffMarketing = IsValidAffMarketing(clickTrackingEvent.AffCode);
                }
            }

            if (validateResult.IsValid)
            {
                bool isMarketingLandingPage = marketingPageCodeRegexPatterns.ToList().Exists(
                    reg => RegexHelper.IsMatch(clickTrackingEvent.PageCode, reg, RegexOptions.IgnoreCase));

                string mappingPageCode = clickTrackingEvent.PageCode;
                if (isMarketingLandingPage)
                {
                    mappingPageCode = validateResult.IsAffMarketing ? clickTrackingEvent.PageCode : LandingPage.JOIN_PAGE_CODE;
                }

                validateResult.RedirectView = HandleRedirectLink(validateResult.RedirectView, clickTrackingEvent.PageCode, clickTrackingEvent.LanguageCode, mappingPageCode);
            }

            return validateResult;
        }

        private async Task<ValidateResult> ValidateLinkFromCache(ClickTrackingEvent clickTrackingEvent)
        {
            var masterInfoValidation = await trackingSystemService.GetMasterDataForValidateClicked().ConfigureAwait(false);
            if (string.IsNullOrEmpty(clickTrackingEvent.AffCode) || string.IsNullOrEmpty(clickTrackingEvent.LanguageCode))
            {
                return new ValidateResult { IsValid = false, ErrorMessage = LinkIsInvalidMessage };
            }

            var validateResult = ValidateLinkSpecialDomain(clickTrackingEvent, masterInfoValidation);
            if (!validateResult.IsValid)
            {
                validateResult = ValidateLinkDomain(clickTrackingEvent, masterInfoValidation);
            }

            if (validateResult.IsValid)
            {
                var commGroup = masterInfoValidation.AffCommGroups.FirstOrDefault(m => m.AffCode == clickTrackingEvent.AffCode);
                if (commGroup != null)
                {
                    validateResult.IsAffMarketing = marketingForCommGroups.ToList().Exists(c => c.MatchWith(commGroup.DealName));
                }
            }

            return validateResult;
        }

        private ValidateResult ValidateLinkDomain(ClickTrackingEvent clickTrackingEvent, MasterInfoValidation masterInfoValidation)
        {
            var affCommGroup = masterInfoValidation.AffCommGroups.FirstOrDefault(m => m.AffCode == clickTrackingEvent.AffCode);
            if (affCommGroup == null)
            {
                return new ValidateResult { IsValid = false, ErrorMessage = LinkIsInvalidMessage };
            }

            var domain = masterInfoValidation.DomainMappings.FirstOrDefault(m => m.AffDomain == clickTrackingEvent.AffDomain
                                    && m.LanguageCode == clickTrackingEvent.LanguageCode);
            if (domain == null)
            {
                return new ValidateResult { IsValid = false, ErrorMessage = LinkIsInvalidMessage };
            }

            var licenseeDomain = $"{(domain.IsSSL ? "https" : "http")}://{domain.RedirectDomain}/{domain.LanguageLink}/{clickTrackingEvent.PageCode}?affCode={affCommGroup.AffCode}";
            return new ValidateResult { IsValid = true, RedirectView = licenseeDomain };
        }

        private ValidateResult ValidateLinkSpecialDomain(ClickTrackingEvent clickTrackingEvent, MasterInfoValidation masterInfoValidation)
        {
            var domain = masterInfoValidation.SpecialDomainMappings.FirstOrDefault(m => m.AffCode == clickTrackingEvent.AffCode
                && m.AffDomain == clickTrackingEvent.AffDomain);
            if (domain == null)
            {
                return new ValidateResult { IsValid = false, ErrorMessage = LinkIsInvalidMessage };
            }

            var languageLink = masterInfoValidation.LanguageReferences.FirstOrDefault(m => m.LanguageCode == clickTrackingEvent.LanguageCode);
            var specialDomain = $"{(domain.IsSSL ? "https" : "http")}://{domain.RedirectDomain}/{languageLink.LanguageLink}/{clickTrackingEvent.PageCode}?affCode={domain.AffCode}";
            return new ValidateResult { IsValid = true, RedirectView = specialDomain };
        }

        private static string GetSupportedPageCodeOnV2(string pageCode)
        {
            if (string.IsNullOrWhiteSpace(pageCode))
            {
                return string.Empty;
            }

            string pageCodeLower = pageCode.ToLowerInvariant();

            return LandingPage.SupportedPageCodeOnV1.TryGetValue(pageCodeLower, out string pageCodeOnV2)
                ? pageCodeOnV2
                : pageCodeLower;
        }

        private bool IsValidAffMarketing(string affCode)
        {
            var commGroup = affInfoService.GetCommGroupByAffCode(affCode);
            return commGroup != null && marketingForCommGroups.ToList().Exists(c => c.MatchWith(commGroup.GroupName));
        }

        private string GetFullAffDomain(string urlWithoutProtocol, string endPointOfAffDomain)
        {
            if (string.IsNullOrEmpty(urlWithoutProtocol) || string.IsNullOrEmpty(endPointOfAffDomain))
            {
                return string.Empty;
            }

            string resultUrl = urlWithoutProtocol;
            if (isRemovePortFromUrl && Uri.TryCreate($"https://{urlWithoutProtocol}", UriKind.Absolute, out var uri))
            {
                resultUrl = urlWithoutProtocol.ReplaceStr($":{uri.Port}", string.Empty, StringComparison.Ordinal);
            }

            int pos = resultUrl.IndexOf($"/{endPointOfAffDomain}", StringComparison.OrdinalIgnoreCase);
            return pos > 0 ? resultUrl.Substring(0, pos) : resultUrl;
        }

        private string GetAffMediaLinkWithoutProtocol()
        {
            string host = Request.Host.Value ?? string.Empty;
            string path = Request.Path.Value ?? string.Empty;
            return $"{host}{path}";
        }

        private static string GetSupportedLanguageCodeOnMemberSiteV2(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return string.Empty;
            }

            return Language.SupportedLanguageCodes.TryGetValue(languageCode.ToLowerInvariant(), out string languageCodeOnV2)
                ? languageCodeOnV2
                : languageCode;
        }

        private static string GetMappingRemovedLanguageCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return string.Empty;
            }

            return Language.RemovedLanguageCodes.TryGetValue(languageCode.ToLowerInvariant(), out string mappingLanguageCode)
                ? mappingLanguageCode
                : languageCode;
        }

        private string HandleRedirectLink(string redirectLink, string pageCode, string languageCode, string mappingPageCode)
        {
            if (string.IsNullOrEmpty(redirectLink) || string.IsNullOrEmpty(pageCode) || string.IsNullOrEmpty(languageCode))
            {
                return redirectLink ?? string.Empty;
            }

            string customLanguageCode = GetSupportedLanguageCodeOnMemberSiteV2(languageCode);
            string redirectPattern = configuration[$"AppSettings:Redirect_{pageCode.ToLowerInvariant()}_pattern"];

            if (!string.IsNullOrEmpty(redirectPattern) && Uri.TryCreate(redirectLink, UriKind.Absolute, out var redirectUri))
            {
                string pathAndQueryString = redirectPattern.ReplaceStr("#language#", customLanguageCode);
                return $"{redirectUri.Scheme}://{redirectUri.Host}{pathAndQueryString}{redirectUri.Query}";
            }

            if (languageCode.MatchWith(Language.BENGALI_SHORTED_CODE))
            {
                return redirectLink.ReplaceStr($"/{pageCode}", $"/{mappingPageCode}").ReplaceStr($"/bn/", $"/bd/");
            }

            return redirectLink
                .ReplaceStr($"/{pageCode}", $"/{mappingPageCode}")
                .ReplaceStr($"/{languageCode}/", $"/{customLanguageCode}/");
        }

        private byte GetDeviceType()
        {
            return httpContextService.IsMobile() ? (byte)1 : (byte)0;
        }
    }
}