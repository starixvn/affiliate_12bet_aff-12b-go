using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Commands;
using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Data.Repository;
using ONEbook.UM.Services;

namespace Fanex.Affiliate.Link.Consumer;

public class ClickTrackingConsumer
{
    private readonly IBus bus;
    private readonly IDynamicRepository dynamicRepository;
    private readonly ILogService logService;
    private readonly int maxRetryCount;
    private readonly Exchange retryExchange;
    private readonly Exchange errorExchange;
    private readonly Queue mainQueue;
    private readonly Queue errorQueue;
    private readonly string routingKey;

    public ClickTrackingConsumer(
        IBus bus,
        IDynamicRepository dynamicRepository,
        ILogService logService,
        int maxRetryCount,

        Exchange retryExchange,
        Exchange errorExchange,
        Queue mainQueue,
        Queue errorQueue,
        string routingKey)
    {
        this.bus = bus;
        this.dynamicRepository = dynamicRepository;
        this.logService = logService;
        this.maxRetryCount = maxRetryCount;
     
        this.retryExchange = retryExchange;
        this.errorExchange = errorExchange;
        this.mainQueue = mainQueue;
        this.errorQueue = errorQueue;
        this.routingKey = routingKey;
    }

    public ClickTrackingConsumer(
        IBus bus,
        IDynamicRepository dynamicRepository,
        ILogService logService,
        int maxRetryCount,

        Queue mainQueue,
        Queue errorQueue,
        string routingKey)
    {
        this.bus = bus;
        this.dynamicRepository = dynamicRepository;
        this.logService = logService;
        this.maxRetryCount = maxRetryCount;


        this.mainQueue = mainQueue;
        this.errorQueue = errorQueue;
        this.routingKey = routingKey;
    }

    public IDisposable StartConsumer(Func<bool> isCurrentlyUnderMaintenance)
    {
        return bus.Advanced.Consume<ClickTrackingEvent>(mainQueue, async (message, _) =>
        {
            if (isCurrentlyUnderMaintenance())
            {
                logService.LogFileMessage("Maintenance mode active – message retained in queue.");
                await Task.FromResult(AckStrategies.NackWithRequeue);
                return;
            }

            try
            {
                logService.LogFileMessage($"[Consumer] Processing: {message.Body.ToString()}");
                await InsertClickInfo(message.Body);
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

    public IDisposable StartDLQConsumer(Func<bool> isCurrentlyUnderMaintenance, Func<bool>stopConsumer)
    {
        return bus.Advanced.Consume<ClickTrackingEvent>(errorQueue, async (message, _) =>
        {
            if (isCurrentlyUnderMaintenance())
            {
                logService.LogFileMessage("Maintenance mode active – message retained in queue.");
                await Task.FromResult(AckStrategies.NackWithRequeue);
                return;
            }

            try
            {
                logService.LogFileMessage($"[Consumer] Processing: {message.Body.ToString()}");
                await InsertClickInfo(message.Body);
                await Task.FromResult(AckStrategies.Ack);
            }
            catch (Exception ex)
            {
                if (isCurrentlyUnderMaintenance())
                {
                    logService.LogFileMessage("Maintenance mode active – message retained in queue.");
                }
                else
                {
                    await logService.SentryErrorAsync($"Processor DLQ message error.", ex);
                }

                stopConsumer();
                await Task.FromResult(AckStrategies.NackWithRequeue);
            }
        });
    }

    private async Task InsertClickInfo(ClickTrackingEvent clickTrackingEvent)
    {
        if (clickTrackingEvent is null)
        {
            logService.LogFileMessage("ClickTrackingEvent is null. Skipping insert.");
            return;
        }

        var command = new ClickTrackingEventCommand
        {
            ClickId = clickTrackingEvent.ClickId,
            AffCode = clickTrackingEvent.AffCode,
            UpdatedTime = clickTrackingEvent.UpdatedTime,
            IP = clickTrackingEvent.IP,
            Browser = clickTrackingEvent.Browser,
            URLReferrer = clickTrackingEvent.UrlReferrer,
            DomainReferrer = clickTrackingEvent.DomainReferrer,
            OS = clickTrackingEvent.OS,
            Device = clickTrackingEvent.Device,
            BrowserVersion = clickTrackingEvent.BrowserVersion,
            BrowserResolution = clickTrackingEvent.BrowserResolution,
            BrowserLanguage = clickTrackingEvent.BrowserLanguage,
            UserAgent = clickTrackingEvent.UserAgent,
            Country = clickTrackingEvent.Country,
            LanguageCode = clickTrackingEvent.LanguageCode,
            PageCode = clickTrackingEvent.PageCode,
            RedirectURL = clickTrackingEvent.RedirectLink,
            DeviceType = clickTrackingEvent.DeviceType,
            AffMediaLink = clickTrackingEvent.AffMediaLink,
            ChannelCode = clickTrackingEvent.ChannelCode
        };

        await dynamicRepository.ExecuteAsync(command);
    }
}
