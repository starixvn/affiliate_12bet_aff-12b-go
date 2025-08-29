namespace Fanex.Affiliate.Link.Services.Impl
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Affiliate.Link.Commands;
    using Fanex.Affiliate.Link.Constants;
    using Fanex.Affiliate.Link.Core.Models;
    using Fanex.Affiliate.Link.Core.Services.Interfaces;
    using Fanex.Affiliate.Link.Criterias;
    using Fanex.Affiliate.Link.DTOs;
    using Fanex.Affiliate.Link.Models;
    using Fanex.Affiliate.Link.Services.Interface;
    using Fanex.Data.Repository;

    public class ClickTrackingEventService(IDynamicRepository dynamicRepository,
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
                RedirectLink = command.RedirectPage,
                PageCode = command.PageCode
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
                var results = await dynamicRepository.FetchMultipleAsync<SpecialDomainMapping, DomainMapping, LanguageReference, AffiliateDeal, LandingPageSetting>(criteria);

                masterInfoValidation = new MasterInfoValidation
                {
                    SpecialDomainMappings = results.Item1,
                    DomainMappings = results.Item2,
                    LanguageReferences = results.Item3,
                    AffiliateDeals = results.Item4,
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
