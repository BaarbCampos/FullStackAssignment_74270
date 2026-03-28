using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.OrderApi.Services;
using SportsStore.Shared.Contracts;
using SportsStore.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace SportsStore.OrderApi.Messaging;

public class ShippingCreatedConsumer : BackgroundService
{
    private readonly IOrderService _orderService;

    public ShippingCreatedConsumer(IOrderService orderService)
    {
        _orderService = orderService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "shipping-created",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            Console.WriteLine("📦 Shipping created event received:");
            Console.WriteLine(json);

            var message = JsonSerializer.Deserialize<ShippingCreated>(json);

            if (message == null)
            {
                Console.WriteLine("❌ Failed to deserialize ShippingCreated message.");
                return;
            }

            _orderService.UpdateOrderStatus(message.OrderId, (int)OrderStatus.ShippingCreated);
            _orderService.UpdateOrderStatus(message.OrderId, (int)OrderStatus.Completed);

            Console.WriteLine($"✅ Order {message.OrderId} updated to Completed.");
        };

        channel.BasicConsume(
            queue: "shipping-created",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }
}