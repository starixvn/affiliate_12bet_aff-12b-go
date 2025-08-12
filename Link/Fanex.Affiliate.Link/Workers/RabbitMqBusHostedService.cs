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
        }

        bus = RabbitHutch.CreateBus(connStr);
        bus.Advanced.Connected += (s, e) =>
        {
            logService.LogFileMessage("RabbitMQ connected.");
        };
        bus.Advanced.Disconnected += async (s, e) =>
        {
            await TryConnectToRabbitMqAsync(stoppingToken);
        };

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
                if (bus == null)
                {
                    await logService.SentryErrorAsync("RabbitMQ bus is not initialized");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                var exchangeName = $"{Core.Constants.QueueIdentify}.exchange";
                var queueName = $"{Core.Constants.QueueIdentify}.queue";
                var routingKey = $"{Core.Constants.QueueIdentify}.routing";
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
