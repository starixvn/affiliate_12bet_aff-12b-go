using Fanex.Data.Repository;

namespace Fanex.Affiliate.Link.Criterias;

public class GetClickStatisticCriteria : CriteriaBase
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public override string GetSettingKey() => "Aff_ClickStatisticsPatterns_Sel";
    public override bool IsValid() => ToDate >= FromDate;
}
