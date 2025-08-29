namespace Fanex.Affiliate.Link.Commands;

public class ValidateLinkCommand : BaseNonQueryCommand
{
    public string AffCode { get; set; }

    public string AffDomain { get; set; }

    public string PageCode { get; set; }

    public string LanguageCode { get; set; }

    public string RedirectPage { get; set; }

    public override string GetSettingKey() => "Aff_ClickStatistics_RedirectPage_Sel";

    public override bool IsValid() => true;
}
