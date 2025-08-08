using EasyNetQ;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Fanex.Affiliate.Link.Core.Services;

public class RabbitBus(IRedisService redisService, ILogService logService) : IRabbitBus
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
                    await ExchangeDeclareAsync();
                }

                await advancedBus.PublishAsync(
                       exchange.Value,
                       routingKey: $"{Constants.QueueIdentify}.routing",
                       mandatory: false,
                   new Message<T>(model)
                   );

                result = true;

                logService.LogFileMessage($"Published model {JsonConvert.SerializeObject(model)} successfully!");
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

    private async Task ExchangeDeclareAsync()
    {
        try
        {
            var exchangeName = $"{Constants.QueueIdentify}.exchange";
            exchange = await advancedBus.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
        }
        catch (Exception ex)
        {
            exchange = null;
        }
    }
}
