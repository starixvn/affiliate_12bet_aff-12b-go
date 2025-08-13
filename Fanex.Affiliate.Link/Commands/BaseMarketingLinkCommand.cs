using Fanex.Data.Repository;

namespace Fanex.Affiliate.Link.Commands
{
    public abstract class BaseMarketingLinkCommand : NonQueryCommand
    {
        public int AffId { get; set; }

        public string ChannelCode { get; set; }

        public string LanguageCode { get; set; }

        public string AffMediaLink { get; set; }

        public bool IsOn { get; } = true;
    }
}