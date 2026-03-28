using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.PaymentService.Configuration;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace SportsStore.PaymentService;

public class Worker : BackgroundService
{
    private readonly RabbitMqSettings _settings;

    public Worker(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            Port = _settings.Port
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "inventory-confirmed", durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: "payment-approved", durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: "payment-rejected", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
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

            Console.WriteLine($"⏳ Processing payment for order {inventoryConfirmed.OrderId}...");
            await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);

            var paymentApproved = Random.Shared.Next(0, 100) >= 30;

            if (paymentApproved)
            {
                var approvedEvent = new PaymentApproved
                {
                    OrderId = inventoryConfirmed.OrderId,
                    ApprovedAtUtc = DateTime.UtcNow,
                    Message = "Payment approved successfully."
                };

                var approvedJson = JsonSerializer.Serialize(approvedEvent);
                var approvedBody = Encoding.UTF8.GetBytes(approvedJson);

                channel.BasicPublish(exchange: "", routingKey: "payment-approved", basicProperties: null, body: approvedBody);

                Console.WriteLine("✅ Payment approved event published:");
                Console.WriteLine(approvedJson);
                return;
            }

            var rejectedEvent = new PaymentRejected
            {
                OrderId = inventoryConfirmed.OrderId,
                RejectedAtUtc = DateTime.UtcNow,
                Reason = "Gateway declined the transaction.",
                Message = "Payment rejected."
            };

            var rejectedJson = JsonSerializer.Serialize(rejectedEvent);
            var rejectedBody = Encoding.UTF8.GetBytes(rejectedJson);

            channel.BasicPublish(exchange: "", routingKey: "payment-rejected", basicProperties: null, body: rejectedBody);

            Console.WriteLine("❌ Payment rejected event published:");
            Console.WriteLine(rejectedJson);
        };

        channel.BasicConsume(queue: "inventory-confirmed", autoAck: true, consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}