#nullable enable
using EasyNetQ;
using EasyNetQ.Topology;
using Fanex.Affiliate.Link.Core.Services.Interfaces;

namespace Fanex.Affiliate.Link.Workers;

public class RabbitMqBusHostedService : BackgroundService
{
    private readonly ILogService logService;
    private readonly IConfiguration configuration;

    private IBus? bus;
    private readonly IRabbitBus rabbitBus;

    public RabbitMqBusHostedService(
        ILogService logService,
        IConfiguration configuration,
      IRabbitBus rabbitBus)
    {
        this.logService = logService;
        this.configuration = configuration;
        this.rabbitBus = rabbitBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connStr = configuration.GetValue<string>("RabbitMQConnectionString");
        if (string.IsNullOrEmpty(connStr))
        {
            await logService.SentryErrorAsync("RabbitMQ connection string is not configured");
            return;
        }

        bus = RabbitHutch.CreateBus(connStr);
        if (bus?.Advanced != null)
        {
            bus.Advanced.Connected += (_, _) =>
            {
                logService.LogFileMessage("RabbitMQ connected.");
            };
            bus.Advanced.Disconnected += async (_, _) =>
            {
                await TryConnectToRabbitMqAsync(stoppingToken);
            };
        }

        await TryConnectToRabbitMqAsync(stoppingToken);
    }

    public override void Dispose()
    {
        bus?.Dispose();
        base.Dispose();
    }

    private async Task TryConnectToRabbitMqAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (bus?.Advanced == null)
                {
                    await logService.SentryErrorAsync("RabbitMQ bus is not available");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                const string exchangeName = $"{Core.Constants.QueueIdentify}.exchange";
                const string queueName = $"{Core.Constants.QueueIdentify}.queue";
                const string routingKey = $"{Core.Constants.QueueIdentify}.routing";
                var exchange = await bus.Advanced.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
                var queue = await bus.Advanced.QueueDeclareAsync(queueName, c =>
                {
                    c.AsDurable(true);
                    c.AsExclusive(false);
                    c.AsAutoDelete(false);
                    c.WithArgument("x-queue-type", "quorum");
                });

                await bus.Advanced.BindAsync(exchange, queue, routingKey);
                rabbitBus.SetBus(bus.Advanced);
                break;
            }
            catch (Exception ex)
            {
                await logService.SentryErrorAsync("RabbitMQ connect failed. Retrying in 10s...", ex);
                logService.LogFileError(ex);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
