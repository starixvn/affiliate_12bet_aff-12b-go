namespace Fanex.Affiliate.Link.Core;

public static class Constants
{
    public const string QueueIdentify = "12b.user.click";
    public const string CacheRedisQueueName = "Aff.12B.Go.ClickQueues";
    public const string TurnOnErrorQueueConsumer = "Aff.12B.Go.TurnOnErrorQueueConsumer";

    public const string MainExchangeName = $"{QueueIdentify}.exchange";
    public const string RetryExchangeName = $"{QueueIdentify}.retry.exchange";
    public const string ErrorExchangeName = $"{QueueIdentify}.error.exchange";
    public const string MainQueueName = $"{QueueIdentify}.queue";
    public const string RetryQueueName = $"{QueueIdentify}.retry.queue";
    public const string ErrorQueueName = $"{QueueIdentify}.error.queue";
    public const string RoutingKey = $"{QueueIdentify}.routing";

}
