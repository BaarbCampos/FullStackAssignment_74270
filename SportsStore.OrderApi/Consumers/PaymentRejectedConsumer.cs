using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.OrderApi.Configuration;
using SportsStore.OrderApi.Services;
using SportsStore.Shared.Contracts;
using SportsStore.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace SportsStore.OrderApi.Consumers;

public class PaymentRejectedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;

    public PaymentRejectedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> options)
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            Port = _settings.Port
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "payment-rejected",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                Console.WriteLine("💥 Payment rejected event received:");
                Console.WriteLine(json);

                var paymentRejected = JsonSerializer.Deserialize<PaymentRejected>(json);

                if (paymentRejected == null)
                {
                    Console.WriteLine("❌ Failed to deserialize PaymentRejected message.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                orderService.UpdateOrderStatus(paymentRejected.OrderId, (int)OrderStatus.PaymentFailed);
                orderService.UpdateOrderStatus(paymentRejected.OrderId, (int)OrderStatus.Failed);

                Console.WriteLine($"❌ Order {paymentRejected.OrderId} marked as FAILED.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error processing payment-rejected message:");
                Console.WriteLine(ex.Message);
            }
        };

        channel.BasicConsume(
            queue: "payment-rejected",
            autoAck: true,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}