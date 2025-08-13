namespace Fanex.Affiliate.Link.Commands
{
    public class ToggleMarketingLinkCommand : BaseMarketingLinkCommand
    {
        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public override string GetSettingKey() => "Aff_User_AffMediaLink_ToggleChannel";

        public override bool IsValid() => AffId > 0;
    }
}