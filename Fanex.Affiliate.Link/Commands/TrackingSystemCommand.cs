namespace Fanex.Affiliate.Link.Commands
{
    public class TrackingSystemCommand : BaseNonQueryCommand
    {
        public string AffCode { get; set; }

        public string AffDomain { get; set; }

        public string PageCode { get; set; }

        public string LanguageCode { get; set; }

        public string ChannelCode { get; set; }

        public string IP { get; set; }

        public string Browser { get; set; }

        public string URLReferrer { get; set; }

        public string DomainReferrer { get; set; }

        public string OS { get; set; }

        public string Device { get; set; }

        public string BrowserVersion { get; set; }

        public string BrowserResolution { get; set; }

        public string BrowserLanguage { get; set; }

        public string UserAgent { get; set; }

        public string Country { get; set; }

        public byte DeviceType { get; set; }

        public string RedirectPage { get; set; }

        public long ClickID { get; set; }

        public string AffMediaLink { get; set; }

        public override string GetSettingKey()
        {
            return "Aff_ClickStatistics_Ins";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}