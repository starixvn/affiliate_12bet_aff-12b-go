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
    private IDisposable? consumerHandle;
    private const int JobDelayMinute = 1;
    private bool isRunning = false;

    private bool IsCurrentlyUnderMaintenance()
    {
        var maintenanceTime = maintenanceService.GetUnderMaintenanceTime();
        return maintenanceTime.IsUnderMaintenanceTime;
    }

    private async Task StartConsumer()
     {
        if (isRunning || IsCurrentlyUnderMaintenance()) return;

        isRunning = true;
        try
        {
            while (true)
            {
                var clickTrackingEvent = await redisService.ListLeftPopAsync<ClickTrackingEvent>(Constants.CacheRedisQueueName);
                if (clickTrackingEvent != null)
                {
                    await databaseService.InsertClickInfo(clickTrackingEvent);
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            await logService.SentryErrorAsync(ex);
        }

        isRunning = false;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            logService.LogFileMessage($"ClickTrackingRedisStorageWorker running at: {DateTimeOffset.Now}");

            await StartConsumer();

            await Task.Delay(TimeSpan.FromMinutes(JobDelayMinute), stoppingToken).ConfigureAwait(false);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        consumerHandle?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
