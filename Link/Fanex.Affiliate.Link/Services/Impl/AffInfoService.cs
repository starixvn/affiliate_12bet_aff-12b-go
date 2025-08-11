namespace Fanex.Affiliate.Link.Services.Impl
{
    using Fanex.Affiliate.Link.Constants;
    using Fanex.Affiliate.Link.Criterias;
    using Fanex.Affiliate.Link.DTOs;
    using Fanex.Affiliate.Link.Services.Interface;
    using Fanex.Data.Repository;

    public class AffInfoService(IDynamicRepository dynamicRepository,
            IMemoryCacheService memoryCacheService) : IAffInfoService
    {

        public AffiliateInfo GetAffInfoByAffId(int affId)
        {
            var cacheKey = $"{CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY}_{affId}";
            var affInfo = memoryCacheService.Get<AffiliateInfo>(cacheKey);
            if (affInfo is null)
            {
                var criteria = new GeneralAffiliateInfoCriteria
                {
                    AffId = affId,
                };

                affInfo = dynamicRepository.Get<AffiliateInfo>(criteria);

                memoryCacheService.Set(cacheKey, affInfo);
            }

            return affInfo ?? new AffiliateInfo();
        }

        public AffiliateInfo GetAffInfoByAffCode(string affCode)
        {
            var cacheKey = $"{CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY}_{affCode}";
            var affInfo = memoryCacheService.Get<AffiliateInfo>(cacheKey);
            if (affInfo is null)
            {
                var criteria = new GeneralAffiliateInfoCriteria
                {
                    AffCode = affCode,
                };

                affInfo = dynamicRepository.Get<AffiliateInfo>(criteria);

                memoryCacheService.Set(cacheKey, affInfo);
            }

            return affInfo ?? new AffiliateInfo();
        }

        public CommGroup GetCommGroupByAffCode(string affCode)
        {
            var cacheKey = $"{CacheKey.COMM_GROUP_CACHE_KEY}_{affCode}";
            var commGroup = memoryCacheService.Get<CommGroup>(cacheKey);
            if (commGroup is null)
            {
                var criteria = new CommGroupByAffCodeCriteria
                {
                    AffCode = affCode,
                };

                commGroup = dynamicRepository.Get<CommGroup>(criteria);

                memoryCacheService.Set(cacheKey, commGroup);
            }

            return commGroup ?? new CommGroup();
        }
    }
}
