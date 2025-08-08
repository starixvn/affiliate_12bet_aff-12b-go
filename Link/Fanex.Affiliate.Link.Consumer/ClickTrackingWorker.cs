using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using ONEbook.UM.Services;

namespace Fanex.Affiliate.Link.Consumer;

public sealed class ClickTrackingWorker(
    IBus bus,
    IMaintenanceService maintenanceService,
    IDatabaseService databaseService,
    ILogService logService) : BackgroundService
{
    private IDisposable? consumerHandle;
    private bool isUnderMaintenance;
    private readonly object maintenanceLock = new();
    private const int maxRetryCount = 5;
    private const int timeToReQueue = 30000;

    private Exchange mainExchange;
    private Exchange retryExchange;
    private Exchange errorExchange;
    private Queue mainQueue;
    private Queue retryQueue;
    private Queue errorQueue;

    private readonly string mainExchangeName = $"{Constants.QueueIdentify}.exchange";
    private readonly string retryExchangeName = $"{Constants.QueueIdentify}.retry.exchange";
    private readonly string errorExchangeName = $"{Constants.QueueIdentify}.error.exchange";
    private readonly string mainQueueName = $"{Constants.QueueIdentify}.queue";
    private readonly string retryQueueName = $"{Constants.QueueIdentify}.retry.queue";
    private readonly string errorQueueName = $"{Constants.QueueIdentify}.error.queue";
    private readonly string routingKey = $"{Constants.QueueIdentify}.routing";

    public void EnableMaintenanceMode()
    {
        lock (maintenanceLock)
        {
            if (isUnderMaintenance)
                return;

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
                return;

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
        consumerHandle = bus.Advanced.Consume<ClickTrackingEvent>(mainQueue, async (message, _) =>
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
                logService.LogFileMessage("Maintenance mode active – message retained in queue.");
                var headers = message.Properties.Headers;
                var retry = headers.TryGetValue("x-retry", out var retryObj) && retryObj is not null
                    ? Convert.ToInt32(retryObj)
                    : 0;

                retry++;

                if (retry > maxRetryCount)
                {
                    await logService.SentryErrorAsync($"Moving message to dead-letter queue after {retry} retries.", ex);
                    var errorMsg = new Message<ClickTrackingEvent>(message.Body);
                    errorMsg.Properties.Headers["x-retry"] = retry;
                    await bus.Advanced.PublishAsync(errorExchange, routingKey, false, errorMsg);
                }
                else
                {
                    await logService.SentryErrorAsync($"Retrying message. Attempt {retry}.", ex);
                    var retryMsg = new Message<ClickTrackingEvent>(message.Body);
                    retryMsg.Properties.Headers["x-retry"] = retry;
                    await bus.Advanced.PublishAsync(retryExchange, routingKey, false, retryMsg);
                }

                await Task.FromResult(AckStrategies.Ack);
            }
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10, stoppingToken).ConfigureAwait(false);
        // Setup exchanges and queues
        mainExchange = await bus.Advanced.ExchangeDeclareAsync(mainExchangeName, ExchangeType.Direct);
        retryExchange = await bus.Advanced.ExchangeDeclareAsync(retryExchangeName, ExchangeType.Direct);
        errorExchange = await bus.Advanced.ExchangeDeclareAsync(errorExchangeName, ExchangeType.Direct);

        mainQueue = await bus.Advanced.QueueDeclareAsync(mainQueueName, c => c
            .AsDurable(true)
            .AsExclusive(false)
            .AsAutoDelete(false)
            .WithArgument("x-queue-type", "quorum")
        );

        retryQueue = await bus.Advanced.QueueDeclareAsync(retryQueueName, c => c
            .AsDurable(true)
            .AsExclusive(false)
            .AsAutoDelete(false)
            .WithArgument("x-queue-type", "quorum")
            .WithArgument("x-dead-letter-exchange", mainExchangeName)
            .WithArgument("x-message-ttl", timeToReQueue)
        );

        errorQueue = await bus.Advanced.QueueDeclareAsync(errorQueueName, c => c
            .AsDurable(true)
            .AsExclusive(false)
            .AsAutoDelete(false)
            .WithArgument("x-queue-type", "quorum")
        );

        await bus.Advanced.BindAsync(mainExchange, mainQueue, routingKey);
        await bus.Advanced.BindAsync(retryExchange, retryQueue, routingKey);
        await bus.Advanced.BindAsync(errorExchange, errorQueue, routingKey);

        StartConsumer();

        while (!stoppingToken.IsCancellationRequested)
        {
            logService.LogFileMessage($"Worker running at: {DateTimeOffset.Now}");

            CheckMaintenanceMode();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        consumerHandle?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
