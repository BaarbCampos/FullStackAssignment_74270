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

public class InventoryFailedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;

    public InventoryFailedConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqSettings> options)
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
            queue: "inventory-failed",
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

                Console.WriteLine("❌ Inventory failed event received:");
                Console.WriteLine(json);

                var inventoryFailed = JsonSerializer.Deserialize<InventoryFailed>(json);

                if (inventoryFailed == null)
                {
                    Console.WriteLine("❌ Failed to deserialize InventoryFailed message.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                orderService.UpdateOrderStatus(inventoryFailed.OrderId, (int)OrderStatus.InventoryFailed);
                orderService.UpdateOrderStatus(inventoryFailed.OrderId, (int)OrderStatus.Failed);

                Console.WriteLine($"❌ Order {inventoryFailed.OrderId} marked as Failed due to inventory.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error processing inventory-failed message:");
                Console.WriteLine(ex.Message);
            }
        };

        channel.BasicConsume(
            queue: "inventory-failed",
            autoAck: true,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}