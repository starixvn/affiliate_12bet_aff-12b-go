namespace Fanex.Affiliate.Link.DTOs;

public class MasterInfoValidation
{
    public IEnumerable<SpecialDomainMapping> SpecialDomainMappings { get; set; }

    public IEnumerable<DomainMapping> DomainMappings { get; set; }

    public IEnumerable<LanguageReference> LanguageReferences { get; set; }

    public IEnumerable<AffiliateDeal> AffiliateDeals { get; set; }

    public IEnumerable<LandingPageSetting> LandingPages { get; set; }
}
