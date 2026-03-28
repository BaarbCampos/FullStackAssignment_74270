using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace SportsStore.InventoryService;

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
            queue: "order-submitted",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.QueueDeclare(
            queue: "inventory-confirmed",
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

            Console.WriteLine("📦 Order received:");
            Console.WriteLine(json);

            var orderSubmitted = JsonSerializer.Deserialize<OrderSubmitted>(json);

            if (orderSubmitted is null)
            {
                Console.WriteLine("❌ Failed to deserialize OrderSubmitted message.");
                return;
            }

            var inventoryConfirmed = new InventoryConfirmed
            {
                OrderId = orderSubmitted.OrderId,
                ConfirmedAtUtc = DateTime.UtcNow,
                Message = "Inventory confirmed successfully."
            };

            var confirmedJson = JsonSerializer.Serialize(inventoryConfirmed);
            var confirmedBody = Encoding.UTF8.GetBytes(confirmedJson);

            channel.BasicPublish(
                exchange: "",
                routingKey: "inventory-confirmed",
                basicProperties: null,
                body: confirmedBody
            );

            Console.WriteLine("✅ Inventory confirmed event published:");
            Console.WriteLine(confirmedJson);
        };

        channel.BasicConsume(
            queue: "order-submitted",
            autoAck: true,
            consumer: consumer
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}