using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.OrderApi.Services;
using SportsStore.Shared.Contracts;
using SportsStore.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace SportsStore.OrderApi.Consumers;

public class ShippingCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ShippingCreatedConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "shipping-created",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            Console.WriteLine("📬 Shipping created event received:");
            Console.WriteLine(json);

            var shippingCreated = JsonSerializer.Deserialize<ShippingCreated>(json);

            if (shippingCreated is null)
            {
                Console.WriteLine("❌ Failed to deserialize ShippingCreated message.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            var updated = orderService.UpdateStatus(
                shippingCreated.OrderId,
                OrderStatus.Completed
            );

            if (updated)
            {
                Console.WriteLine($"✅ Order {shippingCreated.OrderId} updated to Completed.");
            }
            else
            {
                Console.WriteLine($"⚠️ Order {shippingCreated.OrderId} not found.");
            }
        };

        channel.BasicConsume(
            queue: "shipping-created",
            autoAck: true,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}