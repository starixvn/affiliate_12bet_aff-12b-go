using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using ONEbook.UM.Services;

namespace Fanex.Affiliate.Link.Consumer;

public sealed class ClickTrackingRedisStorageWorker(
    IMaintenanceService maintenanceService,
    IDatabaseService databaseService,
    IRedisService redisService,
    ILogService logService) : BackgroundService
{
    private const int JobDelayMinute = 1;
    private const int InitialDelayMilliseconds = 10;
    private bool isRunning;

    private bool IsCurrentlyUnderMaintenance()
    {
        var maintenanceTime = maintenanceService.GetUnderMaintenanceTime();
        return maintenanceTime.IsUnderMaintenanceTime;
    }

    private async Task StartConsumer()
    {
        if (isRunning || IsCurrentlyUnderMaintenance())
        {
            return;
        }

        isRunning = true;
        try
        {
            while (true)
            {
                var clickTrackingEvent = await redisService.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName).ConfigureAwait(false);
                if (clickTrackingEvent == null)
                {
                    break;
                }

                var result = await databaseService.InsertClickInfo(clickTrackingEvent).ConfigureAwait(false);
                if (!result.IsSuccessful)
                {
                    await logService.SentryErrorAsync("InsertClickInfo failed", new Exception(result.ErrorMessage)).ConfigureAwait(false);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            await logService.SentryErrorAsync(ex).ConfigureAwait(false);
        }
        finally
        {
            isRunning = false;
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(InitialDelayMilliseconds, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            logService.LogFileMessage($"ClickTrackingRedisStorageWorker running at: {DateTimeOffset.Now}");

            await StartConsumer();

            await Task.Delay(TimeSpan.FromMinutes(JobDelayMinute), stoppingToken).ConfigureAwait(false);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
