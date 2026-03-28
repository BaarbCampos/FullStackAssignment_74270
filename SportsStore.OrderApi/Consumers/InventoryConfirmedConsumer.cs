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

public class InventoryConfirmedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly IMessagePublisher _messagePublisher;

    public InventoryConfirmedConsumer(
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
            queue: "inventory-confirmed",
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

                Console.WriteLine("📦 Inventory confirmed event received:");
                Console.WriteLine(json);

                var inventoryConfirmed = JsonSerializer.Deserialize<InventoryConfirmed>(json);

                if (inventoryConfirmed == null)
                {
                    Console.WriteLine("❌ Failed to deserialize InventoryConfirmed message.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                orderService.UpdateOrderStatus(inventoryConfirmed.OrderId, (int)OrderStatus.InventoryConfirmed);
                orderService.UpdateOrderStatus(inventoryConfirmed.OrderId, (int)OrderStatus.PaymentPending);

                var paymentRequested = new PaymentRequested
                {
                    OrderId = inventoryConfirmed.OrderId
                };

                await _messagePublisher.PublishAsync("payment-requested", paymentRequested);

                Console.WriteLine($"✅ Order {inventoryConfirmed.OrderId} updated to PaymentPending.");
                Console.WriteLine($"💳 Payment requested event published for order {inventoryConfirmed.OrderId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error processing inventory-confirmed message:");
                Console.WriteLine(ex.Message);
            }
        };

        channel.BasicConsume(
            queue: "inventory-confirmed",
            autoAck: true,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}