using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.OrderApi.Configuration;
using SportsStore.OrderApi.Messaging;
using SportsStore.OrderApi.Services;
using SportsStore.Shared.Contracts;
using SportsStore.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace SportsStore.OrderApi.Consumers;

public class PaymentApprovedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly IMessagePublisher _messagePublisher;

    public PaymentApprovedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> options,
        IMessagePublisher messagePublisher)
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _messagePublisher = messagePublisher;
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
            queue: "payment-approved",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                Console.WriteLine("💵 Payment approved event received:");
                Console.WriteLine(json);

                var paymentApproved = JsonSerializer.Deserialize<PaymentApproved>(json);

                if (paymentApproved == null)
                {
                    Console.WriteLine("❌ Failed to deserialize PaymentApproved message.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                orderService.UpdateOrderStatus(paymentApproved.OrderId, (int)OrderStatus.PaymentApproved);
                orderService.UpdateOrderStatus(paymentApproved.OrderId, (int)OrderStatus.ShippingPending);

                var shippingRequested = new ShippingRequested
                {
                    OrderId = paymentApproved.OrderId
                };

                await _messagePublisher.PublishAsync("shipping-requested", shippingRequested);

                Console.WriteLine($"✅ Order {paymentApproved.OrderId} updated to ShippingPending.");
                Console.WriteLine($"📦 Shipping requested event published for order {paymentApproved.OrderId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error processing payment-approved message:");
                Console.WriteLine(ex.Message);
            }
        };

        channel.BasicConsume(
            queue: "payment-approved",
            autoAck: true,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}