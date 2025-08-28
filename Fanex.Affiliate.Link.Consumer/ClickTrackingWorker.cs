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
    private const int InitialDelayMilliseconds = 10;

    private Exchange retryExchange;
    private Exchange errorExchange;
    private Queue mainQueue;

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
                var result = await databaseService.InsertClickInfo(message.Body);
                if (!result.IsSuccessful)
                {
                    var errMessage = await ProcessMessageWhenError(message);
                    await logService.SentryErrorAsync($"{ errMessage } - { result.ErrorMessage }");
                }

                await Task.FromResult(AckStrategies.Ack);
            }
            catch (Exception ex)
            {
                logService.LogFileMessage("Maintenance mode active – message retained in queue.");
                var errMessage = await ProcessMessageWhenError(message);
                await logService.SentryErrorAsync(errMessage, ex);
                await Task.FromResult(AckStrategies.Ack);
            }
        });
    }

    private async Task<string> ProcessMessageWhenError(IMessage<ClickTrackingEvent> message)
    {
        var errorMessage = string.Empty;
        var headers = message.Properties.Headers;
        var retry = headers.TryGetValue("x-retry", out var retryObj) && retryObj is not null
            ? Convert.ToInt32(retryObj)
            : 0;

        retry++;

        if (retry > maxRetryCount)
        {
            errorMessage = $"Moving message to dead-letter queue after {retry} retries.";
            var errorMsg = new Message<ClickTrackingEvent>(message.Body);
            errorMsg.Properties.Headers["x-retry"] = retry;
            await bus.Advanced.PublishAsync(errorExchange, Constants.RoutingKey, false, errorMsg);
        }
        else
        {
            errorMessage = $"Retrying message. Attempt {retry}.";
            var retryMsg = new Message<ClickTrackingEvent>(message.Body);
            retryMsg.Properties.Headers["x-retry"] = retry;
            await bus.Advanced.PublishAsync(retryExchange, Constants.RoutingKey, false, retryMsg);
        }

        return errorMessage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(InitialDelayMilliseconds, stoppingToken).ConfigureAwait(false);
        // Setup exchanges and queues
        var mainExchange = await bus.Advanced.ExchangeDeclareAsync(Constants.MainExchangeName, ExchangeType.Direct);
        retryExchange = await bus.Advanced.ExchangeDeclareAsync(Constants.RetryExchangeName, ExchangeType.Direct);
        errorExchange = await bus.Advanced.ExchangeDeclareAsync(Constants.ErrorExchangeName, ExchangeType.Direct);

        mainQueue = await bus.Advanced.QueueDeclareAsync(Constants.MainQueueName, c => c
            .AsDurable(true)
            .AsExclusive(false)
            .AsAutoDelete(false)
            .WithArgument("x-queue-type", "quorum")
        );

        var retryQueue = await bus.Advanced.QueueDeclareAsync(Constants.RetryQueueName, c => c
            .AsDurable(true)
            .AsExclusive(false)
            .AsAutoDelete(false)
            .WithArgument("x-queue-type", "quorum")
            .WithArgument("x-dead-letter-exchange", Constants.MainExchangeName)
            .WithArgument("x-message-ttl", timeToReQueue)
        );

        var errorQueue = await bus.Advanced.QueueDeclareAsync(Constants.ErrorQueueName, c => c
            .AsDurable(true)
            .AsExclusive(false)
            .AsAutoDelete(false)
            .WithArgument("x-queue-type", "quorum")
        );
        
        await bus.Advanced.BindAsync(mainExchange, mainQueue, Constants.RoutingKey);
        await bus.Advanced.BindAsync(retryExchange, retryQueue, Constants.RoutingKey);
        await bus.Advanced.BindAsync(errorExchange, errorQueue, Constants.RoutingKey);

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
