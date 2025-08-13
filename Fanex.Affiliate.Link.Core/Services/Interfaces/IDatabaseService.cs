using Fanex.Affiliate.Link.Core.Models;

namespace Fanex.Affiliate.Link.Core.Services.Interfaces;

public interface IDatabaseService
{
    Task InsertClickInfo(ClickTrackingEvent clickTrackingEvent);
}
