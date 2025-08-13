namespace Fanex.Affiliate.Link.Commands
{
    public class CheckMarketingLinkCommand : BaseMarketingLinkCommand
    {
        public string AffDomain { get; set; }

        public bool IsExistedTracking { get; set; }

        public bool IsValidDomain { get; set; }

        public override string GetSettingKey() => "Aff_User_AffMediaLink_CheckExistedTracking";

        public override bool IsValid() => AffId > 0;
    }
}