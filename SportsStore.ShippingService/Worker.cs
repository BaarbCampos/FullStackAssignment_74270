using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace SportsStore.ShippingService;

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "shipping-requested",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

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

            Console.WriteLine("🚚 Shipping request received:");
            Console.WriteLine(json);

            var shippingRequested = JsonSerializer.Deserialize<ShippingRequested>(json);

            if (shippingRequested is null)
            {
                Console.WriteLine("❌ Failed to deserialize ShippingRequested message.");
                return;
            }

            var shippingCreated = new ShippingCreated
            {
                OrderId = shippingRequested.OrderId,
                CreatedAtUtc = DateTime.UtcNow,
                TrackingNumber = $"TRK-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Message = "Shipping created successfully."
            };

            var createdJson = JsonSerializer.Serialize(shippingCreated);
            var createdBody = Encoding.UTF8.GetBytes(createdJson);

            channel.BasicPublish(
                exchange: "",
                routingKey: "shipping-created",
                basicProperties: null,
                body: createdBody
            );

            Console.WriteLine("✅ Shipping created event published:");
            Console.WriteLine(createdJson);
        };

        channel.BasicConsume(
            queue: "shipping-requested",
            autoAck: true,
            consumer: consumer
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}