namespace Fanex.Affiliate.Link.Controllers
{
    using Fanex.Affiliate.Link.Attributes;
    using Fanex.Affiliate.Link.Constants;
    using Fanex.Affiliate.Link.Core.Models;
    using Fanex.Affiliate.Link.Core.Services.Interfaces;
    using Fanex.Affiliate.Link.DTOs;
    using Fanex.Affiliate.Link.Extensions;
    using Fanex.Affiliate.Link.Models;
    using Helpers;
    using Microsoft.AspNetCore.Mvc;
    using ONEbook.UM.Services;
    using Services.Interface;
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Home controller for affiliate link.
    /// </summary>
    public class HomeController(
            IClickTrackingEventService trackingSystemService,
            IMaintenanceService maintenanceService,
            IConfiguration configuration,
            IRedisService redisService,
            IHttpContextService httpContextService,
            IBotDetectionService botDetectionService,
            ILogService logService) : Controller
    {
        private const string LinkIsInvalidMessage = "Oops! This link does not exist. Please reach out to the administrator if you need help.";
        private const string LinkIsIncorrectMessage = "Oops! This link is in an incorrect format. Please reach out to the administrator for assistance.";
        private const string LinkIsOutdatedMessage = "Oops! This link is using an outdated format. Please contact the administrator to get it updated.";

        // HTTP Status Codes
        private const int HttpStatusInternalServerError = 500;

        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Error() => View();

        [HttpGet]
        public IActionResult PageNotFound() => View();

        [HttpGet]
        [TokenAuth]
        public async Task<IActionResult> DeleteRedisKey(string redisKey)
        {
            try
            {
                var result = await redisService.RemoveAsync(redisKey);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await logService.SentryErrorAsync(ex).ConfigureAwait(false);
                return StatusCode(HttpStatusInternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [TokenAuth]
        public async Task<IActionResult> CheckBot(string userAgent)
        {
            try
            {
                var botDetectionResult = botDetectionService.DetectBotFromUserAgentWithDetails(userAgent);
                var result = new
                {
                    isBot = botDetectionResult.IsBot,
                    score = botDetectionResult.Score,
                    reason = botDetectionResult.Reason.Trim(),
                    userAgent
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                await logService.SentryErrorAsync(ex).ConfigureAwait(false);
                return StatusCode(HttpStatusInternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [TokenAuth]
        public async Task<IActionResult> DetectBot(DateTime fromDate, DateTime toDate)
        {
            try
            {
                await botDetectionService.DetectBot(fromDate, toDate);
                return Ok();
            }
            catch (Exception ex)
            {
                await logService.SentryErrorAsync(ex).ConfigureAwait(false);
                return StatusCode(HttpStatusInternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [TokenAuth]
        public async Task<IActionResult> SyncMaxClickId(bool isSync = true)
        {
            try
            {
                var result = await trackingSystemService.SyncMaxClickId(isSync);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await logService.SentryErrorAsync(ex).ConfigureAwait(false);
                return StatusCode(HttpStatusInternalServerError, $"An error occurred while syncing max click id: {ex.Message}");
            }
        }

        [HttpGet]
        [TokenAuth]
        public async Task<IActionResult> RefreshCacheData()
        {
            try
            {
                var result = await trackingSystemService.GetMasterInfoFromDatabase();
                return Ok(result);
            }
            catch (Exception ex)
            {
                await logService.SentryErrorAsync(ex).ConfigureAwait(false);
                return StatusCode(HttpStatusInternalServerError, $"An error occurred while Refresh Cache: {ex.Message}");
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
                    clickTrackingEvent.RedirectLink = validateResult.RedirectLink;
                    clickTrackingEvent.PageCode = validateResult.PageCode;
                    return await ProcessLink(clickTrackingEvent).ConfigureAwait(false);
                }

                ViewBag.Result = validateResult.ErrorMessage;
                return View("Link");
            }
            catch (Exception ex)
            {
                _ = logService.SentryErrorAsync(ex).ConfigureAwait(false);
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
            string userAgent = Request.Headers.UserAgent.ToString();
            string acceptLanguage = Request.Headers.AcceptLanguage.ToString();

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

            var isExcluded = botDetectionService.IsBot(HttpContext);
            clickTrackingEvent.IsExcluded = isExcluded;
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
                try
                {
                    validateResult = await ValidateLinkFromCache(clickTrackingEvent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await logService.SentryErrorAsync(ex).ConfigureAwait(false);
                    validateResult = await trackingSystemService.ValidateLinkFromDatabase(clickTrackingEvent).ConfigureAwait(false);
                }
            }
            else
            {
                try
                {
                    validateResult = await trackingSystemService.ValidateLinkFromDatabase(clickTrackingEvent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await logService.SentryErrorAsync(ex).ConfigureAwait(false);
                    validateResult = await ValidateLinkFromCache(clickTrackingEvent).ConfigureAwait(false);
                }
            }

            if (validateResult.IsValid)
            {
                validateResult.RedirectLink = HandleRedirectLink(validateResult.RedirectLink, clickTrackingEvent.PageCode, clickTrackingEvent.LanguageCode);
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

            return validateResult;
        }

        private ValidateResult ValidateLinkDomain(ClickTrackingEvent clickTrackingEvent, MasterInfoValidation masterInfoValidation)
        {
            logService.LogFileMessage("start ValidateLinkDomain");
            if (clickTrackingEvent == null || masterInfoValidation == null)
            {
                return new ValidateResult
                {
                    IsValid = false,
                    ErrorMessage = LinkIsInvalidMessage
                };
            }

            var affDeal = masterInfoValidation.AffiliateDeals
                .FirstOrDefault(deal => deal.AffCode == clickTrackingEvent.AffCode);

            if (affDeal == null)
            {
                logService.LogFileMessage("ValidateLinkDomain: affDeal is null");
                return new ValidateResult
                {
                    IsValid = false,
                    ErrorMessage = LinkIsInvalidMessage
                };
            }

            var domain = masterInfoValidation.DomainMappings
                .FirstOrDefault(mapping =>
                    mapping.AffDomain == clickTrackingEvent.AffDomain &&
                    mapping.LanguageCode == clickTrackingEvent.LanguageCode);

            if (domain == null)
            {
                logService.LogFileMessage($"ValidateLinkDomain: domain is null, AffDomain: {clickTrackingEvent.AffDomain}, LanguageCode: {clickTrackingEvent.LanguageCode}");
                logService.LogFileMessage(JsonSerializer.Serialize(masterInfoValidation.DomainMappings));
                return new ValidateResult
                {
                    IsValid = false,
                    ErrorMessage = LinkIsInvalidMessage
                };
            }

            var landingPage = masterInfoValidation.LandingPages
                .FirstOrDefault(page => string.Equals(page.PageCode, clickTrackingEvent.PageCode, StringComparison.InvariantCultureIgnoreCase));

            clickTrackingEvent.PageCode = GetLandingPageCode(
                landingPage,
                masterInfoValidation,
                affDeal,
                domain,
                clickTrackingEvent.PageCode);

            var protocol = domain.IsSSL ? "https" : "http";
            var redirectUrl = $"{protocol}://{domain.RedirectDomain}/{domain.LanguageLink}/{clickTrackingEvent.PageCode}?affCode={affDeal.AffCode}";

            return new ValidateResult
            {
                IsValid = true,
                RedirectLink = redirectUrl,
                PageCode = clickTrackingEvent.PageCode
            };
        }

        private static string GetLandingPageCode(
            LandingPageSetting landingPage,
            MasterInfoValidation masterInfoValidation,
            AffiliateDeal affDeal,
            DomainMapping domain,
            string requestedPageCode)
        {
            // Helper to get default landing page code
            string GetDefaultLandingPageCode()
            {
                return masterInfoValidation.LandingPages.FirstOrDefault(m => m.IsDefault)?.PageCode;
            }

            if (landingPage == null)
            {
                return GetDefaultLandingPageCode();
            }

            var dealIds = landingPage.DealIds ?? string.Empty;
            var dealIdList = dealIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var dealIdString = affDeal.DealId.ToString();
            bool isDealIdInList = dealIdList.Contains(dealIdString);
            bool containsZero = dealIdList.Contains("0");

            // Clarify logic for exclusion/inclusion and mobile check
            var isExcludedAndInList = landingPage.IsExcluded && isDealIdInList;
            var isIncludedAndNotInList = !landingPage.IsExcluded && !isDealIdInList && !containsZero;
            var isMobileMismatch = landingPage.IsMobile != domain.IsMobile;

            if (isExcludedAndInList || isIncludedAndNotInList || isMobileMismatch)
            {
                return GetDefaultLandingPageCode();
            }

            return requestedPageCode;
        }

        private ValidateResult ValidateLinkSpecialDomain(ClickTrackingEvent clickTrackingEvent, MasterInfoValidation masterInfoValidation)
        {
            var domain = masterInfoValidation.SpecialDomainMappings.FirstOrDefault(m => m.AffDomain == clickTrackingEvent.AffDomain
                && m.AffCode == clickTrackingEvent.AffCode);

            if (domain is null)
            {
                logService.LogFileMessage("ValidateLinkSpecialDomain: domain is null");
                return new ValidateResult { IsValid = false, ErrorMessage = LinkIsInvalidMessage };
            }

            var isValidLandingPage = masterInfoValidation.LandingPages.Any(m => m.PageCode == clickTrackingEvent.PageCode && domain.IsMobile == m.IsMobile);
            if (!isValidLandingPage)
            {
                var defaultLandingPage = masterInfoValidation.LandingPages.FirstOrDefault(m => m.IsDefault);
                clickTrackingEvent.PageCode = defaultLandingPage.PageCode;
            }

            var languageLink = masterInfoValidation.LanguageReferences.FirstOrDefault(m => m.LanguageCode == clickTrackingEvent.LanguageCode);
            var specialDomain = $"{(domain.IsSSL ? "https" : "http")}://{domain.RedirectDomain}/{languageLink.LanguageLink}/{clickTrackingEvent.PageCode}?affCode={clickTrackingEvent.AffCode}";
            return new ValidateResult { IsValid = true, RedirectLink = specialDomain, PageCode = clickTrackingEvent.PageCode };
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
                : pageCode;
        }

        private string GetFullAffDomain(string urlWithoutProtocol, string endPointOfAffDomain)
        {
            if (string.IsNullOrEmpty(urlWithoutProtocol) || string.IsNullOrEmpty(endPointOfAffDomain))
            {
                return string.Empty;
            }

            string resultUrl = urlWithoutProtocol;
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

        private string HandleRedirectLink(string redirectLink, string pageCode, string languageCode)
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
                return redirectLink.ReplaceStr($"/bn/", $"/bd/");
            }

            return redirectLink
                .ReplaceStr($"/{languageCode}/", $"/{customLanguageCode}/");
        }

        private byte GetDeviceType()
        {
            return httpContextService.IsMobile() ? (byte)1 : (byte)0;
        }
    }
}