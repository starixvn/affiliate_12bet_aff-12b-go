using EasyNetQ;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Fanex.Affiliate.Link.Core.Services;

public class RabbitBus(ILogService logService) : IRabbitBus
{
    private IAdvancedBus? advancedBus;
    private Exchange? exchange;

    public void SetBus(IAdvancedBus bus)
    {
        advancedBus = bus;
    }

    public async Task<bool> PublishAsync<T>(T model)
    {
        var result = false;
        if (advancedBus != null && advancedBus.IsConnected) 
        {
            try
            {
                if (!exchange.HasValue)
                {
                    await RegisterQueueAsync();
                }

                if (exchange.HasValue)
                {
                    await advancedBus.PublishAsync(
                           exchange.Value,
                           routingKey: $"{Constants.QueueIdentify}.routing",
                           mandatory: true,
                       new Message<T>(model)
                       ).ConfigureAwait(false);

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
}
