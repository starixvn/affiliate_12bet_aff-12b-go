using Fanex.Affiliate.Link.Core.Services.Interfaces;
using NLog;
using NLog.Web;

namespace Fanex.Affiliate.Link.Core.Services;

public class LogService(Logging.ILogger logger) : ILogService
{
    private static readonly Logger NLog = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

    public void LogFileError(Exception exception)
    {
        NLog.Error(exception);
    }

    public void LogFileMessage(string message)
    {
        NLog.Error(message);
    }

    public async Task SentryErrorAsync(string errMsg, Exception exception)
    {
        await logger.ErrorAsync(errMsg, exception);
    }

    public async Task SentryErrorAsync(string errMsg)
    {
        await logger.ErrorAsync(errMsg);
    }

    public async Task SentryErrorAsync(Exception exception)
    {
        await logger.ErrorAsync(exception.ToString());
    }

    public async Task SentryInfoAsync(string info)
    {
        await logger.InfoAsync(info);
    }
}
