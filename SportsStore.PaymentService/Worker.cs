using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace SportsStore.PaymentService;

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
            queue: "inventory-confirmed",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.QueueDeclare(
            queue: "payment-approved",
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

            Console.WriteLine("💳 Inventory confirmation received:");
            Console.WriteLine(json);

            var inventoryConfirmed = JsonSerializer.Deserialize<InventoryConfirmed>(json);

            if (inventoryConfirmed is null)
            {
                Console.WriteLine("❌ Failed to deserialize InventoryConfirmed message.");
                return;
            }

            var paymentApproved = new PaymentApproved
            {
                OrderId = inventoryConfirmed.OrderId,
                ApprovedAtUtc = DateTime.UtcNow,
                Message = "Payment approved successfully."
            };

            var approvedJson = JsonSerializer.Serialize(paymentApproved);
            var approvedBody = Encoding.UTF8.GetBytes(approvedJson);

            channel.BasicPublish(
                exchange: "",
                routingKey: "payment-approved",
                basicProperties: null,
                body: approvedBody
            );

            Console.WriteLine("✅ Payment approved event published:");
            Console.WriteLine(approvedJson);
        };

        channel.BasicConsume(
            queue: "inventory-confirmed",
            autoAck: true,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}