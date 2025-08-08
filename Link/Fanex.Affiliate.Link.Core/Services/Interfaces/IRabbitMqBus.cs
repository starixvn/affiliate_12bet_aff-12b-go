using EasyNetQ;
using EasyNetQ.Topology;

namespace Fanex.Affiliate.Link.Core.Services.Interfaces;

public interface IRabbitBus
{
    void SetBus(IAdvancedBus bus);
    Task<bool> PublishAsync<T>(T model);
}
