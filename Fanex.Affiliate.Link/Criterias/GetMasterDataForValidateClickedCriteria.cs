using Fanex.Data.Repository;

namespace Fanex.Affiliate.Link.Criterias;

public class GetMasterDataForValidateClickedCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Aff_ClickStatistics_BulkCache";
    public override bool IsValid() => true;
}
