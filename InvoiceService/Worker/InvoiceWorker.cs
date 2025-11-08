using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Shared.Contracts.Events;

public class InvoiceWorker : BackgroundService
{
    private IConnection? conn;
    private IChannel? channel;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        conn = await factory.CreateConnectionAsync();
        channel = await conn.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("pixelz.exchange", ExchangeType.Fanout);
        var queue = await channel.QueueDeclareAsync(queue: "", exclusive: true);
        await channel.QueueBindAsync(queue.QueueName, "pixelz.exchange", "order.checkedout");
        await base.StartAsync(cancellationToken);

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, e) =>
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            var ev = JsonSerializer.Deserialize<OrderCheckedOutEvent>(body);
            Console.WriteLine($"[Invoice] Creating invoice for order {ev?.OrderId} amount {ev?.Amount}");
            // simulate creation
            await channel.BasicAckAsync(deliveryTag: e.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync("", autoAck: false, consumer: consumer);
        await Task.Delay(-1);
    }


    public override Task StopAsync(CancellationToken cancellationToken)
    {
        channel?.CloseAsync();
        conn?.CloseAsync();
        return base.StopAsync(cancellationToken);
    }
}
