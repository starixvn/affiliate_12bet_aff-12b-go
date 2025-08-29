using Fanex.Affiliate.Link.DTOs;

namespace Fanex.Affiliate.Link.Services.Interface
{
    public interface IAffInfoService
    {
        AffiliateInfo GetAffInfoByAffId(int affId);

        AffiliateInfo GetAffInfoByAffCode(string affCode);

        CommGroup GetCommGroupByAffCode(string affCode);
    }
}
