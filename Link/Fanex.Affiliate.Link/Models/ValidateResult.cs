namespace Fanex.Affiliate.Link.Models;

public class ValidateResult
{
    public bool IsValid { get; set; }

    public string ErrorMessage { get; set; }

    public string RedirectLink { get; set; }

    public bool IsAffMarketing { get; set; }
}
