using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Serilog;

namespace Fanex.Affiliate.Link.Core.Services;

public class LogService(Logging.ILogger logger) : ILogService
{
    public void LogFileError(Exception exception)
    {
        Log.Error(exception, "An error occurred");
    }

    public void LogFileMessage(string message)
    {
        Log.Information(message);
    }

    public void LogFileMessageWithProperties(string message, Dictionary<string, object> properties)
    {
        Log.Information("{Message} {@Properties}", message, properties);
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
