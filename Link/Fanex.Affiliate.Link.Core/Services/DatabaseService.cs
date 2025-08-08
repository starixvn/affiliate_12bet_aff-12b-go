using Fanex.Affiliate.Link.Commands;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Data.Repository;

namespace Fanex.Affiliate.Link.Core.Services;

public class DatabaseService(IDynamicRepository dynamicRepository) : IDatabaseService
{
    public async Task InsertClickInfo(ClickTrackingEvent clickTrackingEvent)
    {
        var command = new ClickTrackingEventCommand
        {
            ClickId = clickTrackingEvent.ClickId,
            AffCode = clickTrackingEvent.AffCode,
            UpdatedTime = clickTrackingEvent.UpdatedTime,
            IP = clickTrackingEvent.IP,
            Browser = clickTrackingEvent.Browser,
            URLReferrer = clickTrackingEvent.UrlReferrer,
            DomainReferrer = clickTrackingEvent.DomainReferrer,
            OS = clickTrackingEvent.OS,
            Device = clickTrackingEvent.Device,
            BrowserVersion = clickTrackingEvent.BrowserVersion,
            BrowserResolution = clickTrackingEvent.BrowserResolution,
            BrowserLanguage = clickTrackingEvent.BrowserLanguage,
            UserAgent = clickTrackingEvent.UserAgent,
            Country = clickTrackingEvent.Country,
            LanguageCode = clickTrackingEvent.LanguageCode,
            PageCode = clickTrackingEvent.PageCode,
            RedirectURL = clickTrackingEvent.RedirectLink,
            DeviceType = clickTrackingEvent.DeviceType,
            AffMediaLink = clickTrackingEvent.AffMediaLink,
            ChannelCode = clickTrackingEvent.ChannelCode
        };

        await dynamicRepository.ExecuteAsync(command);
    }
}
