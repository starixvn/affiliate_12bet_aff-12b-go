namespace Fanex.Affiliate.Link.Services.Impl
{
    using Fanex.Affiliate.Link.Commands;
    using Fanex.Affiliate.Link.Constants;
    using Fanex.Affiliate.Link.Core.Models;
    using Fanex.Affiliate.Link.Core.Services.Interfaces;
    using Fanex.Affiliate.Link.Criterias;
    using Fanex.Affiliate.Link.DTOs;
    using Fanex.Affiliate.Link.Models;
    using Fanex.Affiliate.Link.Services.Interface;
    using Fanex.Data.Repository;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ClickTrackingEventService(IDynamicRepository dynamicRepository,
        IMemoryCacheService memoryCacheService,
        IRabbitBus rabbitBus,
        IRedisService redisService,
        IDatabaseService databaseService,
        ILogService logService) : IClickTrackingEventService
    {
        private const int CacheDurationInSeconds = 86400;

        public async Task<string> InsertStatistic(TrackingSystemCommand command)
        {
            await dynamicRepository.ExecuteAsync(command);

            return command.RedirectPage;
        }

        public async Task<CheckMarketingLinkResult> GetTrackingMarketingLinkInfo(CheckMarketingLinkCommand command)
        {
            var cacheKey = $"{CacheKey.MEDIA_LINK_CACHE_KEY}_{command.AffMediaLink}";
            var checkMarketingLinkInfo = memoryCacheService.Get<CheckMarketingLinkResult>(cacheKey);
            if (checkMarketingLinkInfo is null || (checkMarketingLinkInfo.IsValidDomain && !checkMarketingLinkInfo.IsExistedTracking))
            {
                await dynamicRepository.ExecuteAsync(command);
                checkMarketingLinkInfo = new CheckMarketingLinkResult
                {
                    IsValidDomain = command.IsValidDomain,
                    IsExistedTracking = command.IsExistedTracking,
                };

                memoryCacheService.Set(cacheKey, checkMarketingLinkInfo);
            }

            return checkMarketingLinkInfo;
        }

        public async Task<bool> ToggleMarketingLink(ToggleMarketingLinkCommand command)
        {
            await dynamicRepository.ExecuteAsync(command);

            return command.ErrCode == 0;
        }

        public async Task<IEnumerable<MarketInfo>> GetMarkets()
        {
            var markets = memoryCacheService.Get<IEnumerable<MarketInfo>>(CacheKey.SUPPORTED_MARKET_CACHE_KEY);
            if (markets is null)
            {
                var criteria = new BaseCriteria("Aff_Market_Sel");
                markets = await dynamicRepository.FetchAsync<MarketInfo>(criteria);

                memoryCacheService.Set(CacheKey.SUPPORTED_MARKET_CACHE_KEY, markets);
            }

            return markets ?? new List<MarketInfo>();
        }

        public async Task<MarketInfo> GetMarketInfo(string lang)
        {
            var cacheKey = $"{CacheKey.MARKET_INFO_CACHE_KEY}_{lang}";
            var marketInfo = memoryCacheService.Get<MarketInfo>(cacheKey);
            if (marketInfo is null)
            {
                var markets = await GetMarkets();
                marketInfo = markets.FirstOrDefault(m => string.Equals(lang, m.ShortedCode, StringComparison.OrdinalIgnoreCase));

                memoryCacheService.Set(cacheKey, marketInfo);
            }

            return marketInfo ?? new MarketInfo();
        }

        public async Task<ValidateResult> ValidateLinkFromDatabase(ClickTrackingEvent clickTrackingEvent)
        {
            var command = new ValidateLinkCommand
            {
                AffCode = clickTrackingEvent.AffCode,
                AffDomain = clickTrackingEvent.AffDomain,
                PageCode = clickTrackingEvent.PageCode,
                LanguageCode = clickTrackingEvent.LanguageCode
            };
            await dynamicRepository.ExecuteAsync(command);

            var validateResult = new ValidateResult
            {
                IsValid = command.ErrCode == 0,
                ErrorMessage = command.ErrMsg,
                RedirectView = command.RedirectPage,
            };
            return validateResult;
        }

        public async Task<MasterInfoValidation> GetMasterDataForValidateClicked()
        {
            const string cacheKey = CacheKey.MASTER_DATA;
            var slidingExpirationTime = TimeSpan.FromSeconds(CacheDurationInSeconds);
            var masterInfoValidation = await redisService.GetAsync<MasterInfoValidation>(cacheKey);

            if (masterInfoValidation is null)
            {
                var criteria = new GetMasterDataForValidateClickedCriteria();
                var results = await dynamicRepository.FetchMultipleAsync<SpecialDomainMapping, DomainMapping, LanguageReference, AffiliateCommGroup, LandingPageSetting>(criteria);

                masterInfoValidation = new MasterInfoValidation
                {
                    SpecialDomainMappings = results.Item1,
                    DomainMappings = results.Item2,
                    LanguageReferences = results.Item3,
                    AffCommGroups = results.Item4,
                    LandingPages = results.Item5
                };

                await redisService.SetAsync(cacheKey, masterInfoValidation, slidingExpirationTime);
            }

            return masterInfoValidation;
        }

        public async Task<long> GetClickId()
        {
            var clickIdFromCache = await redisService.StringGetAsync(CacheKey.CLICKID);
            if (clickIdFromCache is null)
            {
                await SyncMaxClickId(true);
            }

            var result = await redisService.StringIncrementAsync(CacheKey.CLICKID);
            return result;
        }

        public async Task<long> SyncMaxClickId(bool isSync)
        {
            const int ClickIdIncrement = 100;

            var clickIdFromDatabase = await GetMaxClickIdFromDatabase();
            clickIdFromDatabase += ClickIdIncrement;
            if (isSync)
            {
                await redisService.StringSetAsync(CacheKey.CLICKID, clickIdFromDatabase.ToString());
            }

            return clickIdFromDatabase;
        }

        public async Task PublishMessage(ClickTrackingEvent clickTrackingEvent)
        {
            var isSuceess = await rabbitBus.PublishAsync(clickTrackingEvent);
            if (!isSuceess)
            {
                try
                {
                    await redisService.ListRightPushAsync(CacheKey.CLICKQUEUES, clickTrackingEvent);
                }
                catch (Exception ex)
                {
                    await logService.SentryErrorAsync(ex);
                    await databaseService.InsertClickInfo(clickTrackingEvent);
                }
            }
        }

        private async Task<long> GetMaxClickIdFromDatabase()
        {
            var criteria = new GetMaxClickIdCriteria();
            var clickId = await dynamicRepository.GetAsync<long>(criteria);
            return clickId;
        }
    }
}
