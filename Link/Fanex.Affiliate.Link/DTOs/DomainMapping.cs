namespace Fanex.Affiliate.Link.DTOs;

public class DomainMapping
{
    public string AffDomain { get; set; }

    public string LanguageCode { get; set; }

    public string LanguageLink { get; set; }

    public string RedirectDomain { get; set; }

    public bool IsSSL { get; set; }

    public bool IsMobile { get; set; }

    public bool IsExcluded { get; set; }

    public string DealIds { get; set; }
}
