using Microsoft.AspNetCore.Http;

namespace Fanex.Affiliate.Link.Core.Helpers;

public static class HttpContextHelper
{
    public static string GetClientIpAddress(HttpContext httpContext)
    {
        var realIp = httpContext.Request.Headers["x-actual-ip"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }
        else
        {
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}
