namespace Fanex.Affiliate.Link.Core.Services.Interfaces;

public interface ILogService
{
    Task SentryErrorAsync(string errMsg, Exception exception);

    Task SentryErrorAsync(string errMsg);

    Task SentryErrorAsync(Exception exception);

    Task SentryInfoAsync(string info);

    void LogFileMessage(string message);

    void LogFileError(Exception exception);

    void LogFileMessageWithProperties(string message, Dictionary<string, object> properties);
}
