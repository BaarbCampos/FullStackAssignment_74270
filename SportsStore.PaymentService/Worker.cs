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
        Console.WriteLine("✅ PaymentService worker started");
        Console.WriteLine("👂 Listening on queue: payment-requested");

        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            Port = _settings.Port
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "payment-requested",
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

        channel.QueueDeclare(
            queue: "payment-rejected",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            Console.WriteLine("💳 Payment request received:");
            Console.WriteLine(json);

            var paymentRequested = JsonSerializer.Deserialize<PaymentRequested>(json);

            if (paymentRequested is null)
            {
                Console.WriteLine("❌ Failed to deserialize PaymentRequested message.");
                return;
            }

            Console.WriteLine($"⏳ Processing payment for order {paymentRequested.OrderId}...");
            await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);

            var isApproved = Random.Shared.Next(0, 100) >= 30;

            if (isApproved)
            {
                var approvedEvent = new PaymentApproved
                {
                    OrderId = paymentRequested.OrderId,
                    ApprovedAtUtc = DateTime.UtcNow,
                    Message = "Payment approved successfully."
                };

                var approvedJson = JsonSerializer.Serialize(approvedEvent);
                var approvedBody = Encoding.UTF8.GetBytes(approvedJson);

                channel.BasicPublish(
                    exchange: "",
                    routingKey: "payment-approved",
                    basicProperties: null,
                    body: approvedBody
                );

                Console.WriteLine("✅ Payment approved event published:");
                Console.WriteLine(approvedJson);
                return;
            }

            var rejectedEvent = new PaymentRejected
            {
                OrderId = paymentRequested.OrderId,
                RejectedAtUtc = DateTime.UtcNow,
                Reason = "Gateway declined the transaction.",
                Message = "Payment rejected."
            };

            var rejectedJson = JsonSerializer.Serialize(rejectedEvent);
            var rejectedBody = Encoding.UTF8.GetBytes(rejectedJson);

            channel.BasicPublish(
                exchange: "",
                routingKey: "payment-rejected",
                basicProperties: null,
                body: rejectedBody
            );

            Console.WriteLine("❌ Payment rejected event published:");
            Console.WriteLine(rejectedJson);
        };

        channel.BasicConsume(
            queue: "payment-requested",
            autoAck: true,
            consumer: consumer
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}