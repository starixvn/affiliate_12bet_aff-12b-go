namespace Fanex.Affiliate.Link.Services.Interface
{
    /// <summary>
    /// Service interface for HTTP context-dependent operations.
    /// </summary>
    public interface IHttpContextService
    {
        /// <summary>
        /// Get Country from the HTTP request.
        /// </summary>
        /// <returns>Country English Name.</returns>
        string GetCountry();

        /// <summary>
        /// Get original protocol from the request, considering Cloudflare and proxy headers.
        /// </summary>
        /// <returns>The protocol (scheme) as a string, e.g., "http" or "https".</returns>
        string GetOriginalProtocol();

        /// <summary>
        /// Get device info from user agent and HTTP request.
        /// </summary>
        /// <param name="userAgent">User's info.</param>
        /// <returns>Device info.</returns>
        string GetDeviceName(string userAgent);

        /// <summary>
        /// Determines if the current request is from a mobile device.
        /// Enhanced for ASP.NET Core compatibility and robustness.
        /// </summary>
        /// <returns>True if mobile device, otherwise false.</returns>
        bool IsMobile();
    }
}