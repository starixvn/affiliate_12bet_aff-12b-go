using Fanex.Data.Repository;

namespace Fanex.Affiliate.Link.Criterias
{
    public class CommGroupByAffCodeCriteria : NonQueryCommand
    {
        public string AffCode { get; set; }

        public override string GetSettingKey()
        {
            return "Aff_AffiliateUser_ExtraUserInfoSel";
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(AffCode);
        }
    }
}