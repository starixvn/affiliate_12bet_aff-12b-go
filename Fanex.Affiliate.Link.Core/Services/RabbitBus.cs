using EasyNetQ;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Fanex.Affiliate.Link.Core.Services;

public class RabbitBus(ILogService logService) : IRabbitBus
{
    private IAdvancedBus? advancedBus;
    private Exchange? exchange;

    public bool IsConnected => advancedBus?.IsConnected ?? false;

    public void SetBus(IAdvancedBus bus)
    {
        advancedBus = bus;
    }

    public async Task<bool> PublishAsync<T>(T model)
    {
        var result = false;
        if (IsConnected)
        {
            try
            {
                if (!exchange.HasValue)
                {
                    await RegisterQueueAsync();
                }

                if (exchange.HasValue)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    await advancedBus.PublishAsync(
                           exchange.Value,
                           routingKey: $"{Constants.QueueIdentify}.routing",
                           mandatory: true,
                       new Message<T>(model)
                       ).ConfigureAwait(false);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    result = true;

                    logService.LogFileMessage($"Published model {JsonConvert.SerializeObject(model)} successfully!");
                }
            }
            catch (Exception ex)
            {
                await logService.SentryErrorAsync(
                    $"Error publishing object: {JsonConvert.SerializeObject(model)}.", ex);

                exchange = null;
                logService.LogFileMessage($"Message queued for later publishing after error: {typeof(T).Name}");
            }
        }

        return result;
    }

    public async Task RegisterQueueAsync()
    {
        const string exchangeName = $"{Core.Constants.QueueIdentify}.exchange";
        const string queueName = $"{Core.Constants.QueueIdentify}.queue";
        const string routingKey = $"{Core.Constants.QueueIdentify}.routing";
        exchange = await advancedBus.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
        var queue = await advancedBus!.QueueDeclareAsync(queueName, c =>
        {
            c.AsDurable(true);
            c.AsExclusive(false);
            c.AsAutoDelete(false);
            c.WithArgument("x-queue-type", "quorum");
        });

        await advancedBus.BindAsync(exchange.Value, queue, routingKey);
    }

    public async Task<Dictionary<string, object>> GetConnectionInfoAsync()
    {
        var info = new Dictionary<string, object>
        {
            ["IsConnected"] = IsConnected,
            ["HasAdvancedBus"] = advancedBus != null,
            ["HasExchange"] = exchange.HasValue,
            ["LastCheck"] = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd HH:mm:ss")
        };

        if (advancedBus != null)
        {
            try
            {
                info["ConnectionState"] = advancedBus.IsConnected ? "Connected" : "Disconnected";

                if (advancedBus.IsConnected)
                {
                    info["ConnectionActive"] = true;
                    info["ConnectionType"] = "EasyNetQ.IAdvancedBus";
                }
            }
            catch (Exception ex)
            {
                info["Error"] = ex.Message;
                await logService.SentryErrorAsync("Failed to get RabbitMQ connection info", ex);
            }
        }

        return info;
    }
}
