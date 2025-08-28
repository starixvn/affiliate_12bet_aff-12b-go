using EasyNetQ;

namespace Fanex.Affiliate.Link.Core.Services.Interfaces;

public interface IRabbitBus
{
    void SetBus(IAdvancedBus bus);
    Task<bool> PublishAsync<T>(T model);
    Task<QueueStats> GetQueueStatsAsync(string queueName);
    Task RegisterQueueAsync();
    bool IsConnected { get; }
    Task<Dictionary<string, object>> GetConnectionInfoAsync();
}
