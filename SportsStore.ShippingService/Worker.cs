using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace SportsStore.ShippingService;

public class Worker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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
            queue: "payment-approved",
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

            Console.WriteLine("🚚 Payment approval received:");
            Console.WriteLine(json);

            var paymentApproved = JsonSerializer.Deserialize<PaymentApproved>(json);

            if (paymentApproved is null)
            {
                Console.WriteLine("❌ Failed to deserialize PaymentApproved message.");
                return;
            }

            var shippingCreated = new ShippingCreated
            {
                OrderId = paymentApproved.OrderId,
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
            queue: "payment-approved",
            autoAck: true,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}