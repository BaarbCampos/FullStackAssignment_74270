using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Shared.Contracts;
using SportsStore.ShippingService.Configuration;
using System.Text;
using System.Text.Json;

namespace SportsStore.ShippingService;

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
        };

        channel.BasicConsume(
            queue: "shipping-requested",
            autoAck: true,
            consumer: consumer
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}