namespace Fanex.Affiliate.Link.Core.Models;

/// <summary>
/// Represents a click tracking event for an affiliate link.
/// </summary>
public class ClickTrackingEvent
{
    public required string AffCode { get; set; }
    public required string AffDomain { get; set; }
    public required string PageCode { get; set; }
    public required string LanguageCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string IP { get; set; }
    public required string Browser { get; set; }
    public required string UrlReferrer { get; set; }
    public required string DomainReferrer { get; set; }
    public required string OS { get; set; }
    public required string Device { get; set; }
    public required string BrowserVersion { get; set; }
    public required string BrowserResolution { get; set; }
    public required string BrowserLanguage { get; set; }
    public required string UserAgent { get; set; }
    public required string Country { get; set; }
    public byte DeviceType { get; set; }
    public required string RedirectLink { get; set; }
    public long ClickId { get; set; }
    public DateTime UpdatedTime { get; set; }
    public required string AffMediaLink { get; set; }
}
