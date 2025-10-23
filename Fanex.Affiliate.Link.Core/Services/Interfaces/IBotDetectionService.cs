using Microsoft.AspNetCore.Http;

namespace Fanex.Affiliate.Link.Core.Services.Interfaces;

public interface IBotDetectionService
{
    bool IsBot(HttpContext context);

    bool DetectBotFromUserAgent(string userAgent);

    Task DetectBot(DateTime fromDate, DateTime toDate);
}
