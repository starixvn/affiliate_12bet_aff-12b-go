using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using SharpRaven;

namespace Fanex.Affiliate.Link;

public static class LoggingConfig
{
    public static void RegisterSentry()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var env = configuration["AppSettings:Environment"];
        var backEndDsn = configuration["AppSettings:SentryBackEndDsn"];
        var category = configuration["AppSettings:LoggingCategory"];
        var isRegisterSentry = string.Equals(configuration["AppSettings:IsRegisterSentry"], "true", StringComparison.CurrentCultureIgnoreCase);

        if (isRegisterSentry && !string.IsNullOrEmpty(backEndDsn))
        {
            LogManager.SetDefaultLogCategory(category).Use(new List<ILogEngine>
            {
                new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(backEndDsn),
                    Environment = env
                })
            });
        }
    }
}