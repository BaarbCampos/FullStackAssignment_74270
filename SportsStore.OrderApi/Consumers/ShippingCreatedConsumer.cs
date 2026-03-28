using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.OrderApi.Services;
using SportsStore.Shared.Contracts;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderApi.Messaging;

public class ShippingCreatedConsumer : BackgroundService
{
    private readonly IOrderService _orderService;

    public ShippingCreatedConsumer(IOrderService orderService)
    {
        _orderService = orderService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare("shipping-created", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var message = JsonSerializer.Deserialize<ShippingCreated>(json);

            Console.WriteLine("📦 Shipping received in Order API:");
            Console.WriteLine(json);

            if (message != null)
            {
                _orderService.UpdateOrderStatus(message.OrderId, (int)OrderStatus.Completed);
            }
        };

        channel.BasicConsume(queue: "shipping-created", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }
}