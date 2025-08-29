using Fanex.Data.Repository;

namespace Fanex.Affiliate.Link.Criterias
{
    public class BaseCriteria : CriteriaBase
    {
        private readonly string SpKey;

        public BaseCriteria(string spKey)
        {
            SpKey = spKey;
        }

        public override string GetSettingKey() => SpKey;

        public override bool IsValid() => !string.IsNullOrEmpty(SpKey);
    }
}