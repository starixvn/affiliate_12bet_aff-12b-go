using Fanex.Data.Repository;

namespace Fanex.Affiliate.Link.Criterias;

public class GetMaxClickIdCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Aff_ClickStatistics_MaxClickId_Sel";
    public override bool IsValid() => true;
}
