using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using ONEbook.UM.Services;

namespace Fanex.Affiliate.Link.Consumer;

public sealed class ClickTrackingDLQWorker(
    IBus bus,
    IMaintenanceService maintenanceService,
    IDatabaseService databaseService,
    ILogService logService) : BackgroundService
{
    private IDisposable? consumerHandle;
    private bool isUnderMaintenance;
    private readonly object maintenanceLock = new();
    private const int timeToRun = 5;
    private const int InitialDelayMilliseconds = 10;
    private bool isRunning;

    private Queue errorQueue;
    private readonly string errorExchangeName = $"{Constants.QueueIdentify}.error.exchange";
    private readonly string errorQueueName = $"{Constants.QueueIdentify}.error.queue";
    private readonly string routingKey = $"{Constants.QueueIdentify}.routing";

    public void EnableMaintenanceMode()
    {
        lock (maintenanceLock)
        {
            if (isUnderMaintenance)
            {
                return;
            }

            isUnderMaintenance = true;
            consumerHandle?.Dispose();
            consumerHandle = null;
            logService.LogFileMessage("Maintenance mode enabled – message processing paused.");
        }
    }

    public void DisableMaintenanceMode()
    {
        lock (maintenanceLock)
        {
            if (!isUnderMaintenance)
            {
                return;
            }

            isUnderMaintenance = false;
            StartConsumer();
            logService.LogFileMessage("Maintenance mode disabled – message processing resumed.");
        }
    }

    public void CheckMaintenanceMode()
    {
        if (IsCurrentlyUnderMaintenance())
        {
            EnableMaintenanceMode();
        }
        else
        {
            DisableMaintenanceMode();
        }
    }

    private bool IsCurrentlyUnderMaintenance()
    {
        var maintenanceTime = maintenanceService.GetUnderMaintenanceTime();
        return maintenanceTime.IsUnderMaintenanceTime;
    }

    private void StartConsumer()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        consumerHandle = bus.Advanced.Consume<ClickTrackingEvent>(errorQueue, async (message, _) =>
        {
            if (IsCurrentlyUnderMaintenance())
            {
                logService.LogFileMessage("Maintenance mode active – message retained in queue.");
                await Task.FromResult(AckStrategies.NackWithRequeue);
                return;
            }

            try
            {
                logService.LogFileMessage($"[Consumer] Processing: {message.Body.ToString()}");
                await databaseService.InsertClickInfo(message.Body);
                await Task.FromResult(AckStrategies.Ack);
            }
            catch (Exception ex)
            {
                if (IsCurrentlyUnderMaintenance())
                {
                    logService.LogFileMessage("Maintenance mode active – message retained in queue.");
                }
                else
                {
                    await logService.SentryErrorAsync($"Processor DLQ message error.", ex);
                }

                StopConsumer();
                await Task.FromResult(AckStrategies.NackWithRequeue);
            }
        });
    }

    private void StopConsumer()
    {
        isRunning = false;
        if (consumerHandle != null)
        {
            consumerHandle?.Dispose();
            consumerHandle = null;
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(InitialDelayMilliseconds, stoppingToken).ConfigureAwait(false);

        // Setup exchanges and queues
        var errorExchange = await bus.Advanced.ExchangeDeclareAsync(errorExchangeName, ExchangeType.Direct);
        errorQueue = await bus.Advanced.QueueDeclareAsync(errorQueueName, c => c
            .AsDurable(true)
            .AsExclusive(false)
            .AsAutoDelete(false)
            .WithArgument("x-queue-type", "quorum")
        );

        await bus.Advanced.BindAsync(errorExchange, errorQueue, routingKey);

        while (!stoppingToken.IsCancellationRequested)
        {
            logService.LogFileMessage($"Worker running at: {DateTimeOffset.Now}");

            StartConsumer();

            await Task.Delay(TimeSpan.FromMinutes(timeToRun), stoppingToken).ConfigureAwait(false);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        consumerHandle?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
