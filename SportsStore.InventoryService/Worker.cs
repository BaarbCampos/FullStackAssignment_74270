using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.InventoryService.Configuration;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace SportsStore.InventoryService;

public class Worker : BackgroundService
{
    private readonly RabbitMqSettings _settings;

    public Worker(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

        channel.QueueDeclare("order-submitted", false, false, false);
        channel.QueueDeclare("inventory-confirmed", false, false, false);
        channel.QueueDeclare("inventory-failed", false, false, false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var order = JsonSerializer.Deserialize<OrderSubmitted>(json);

            if (order == null) return;

            var connectionString = "Data Source=../SportsStore.OrderApi/orders.db";

            using var db = new SqliteConnection(connectionString);
            db.Open();

            bool hasStock = true;

            foreach (var item in order.Items)
            {
                var command = db.CreateCommand();
                command.CommandText =
                @"
                SELECT StockQuantity 
                FROM Products 
                WHERE Id = $id
                ";
                command.Parameters.AddWithValue("$id", item.ProductId);

                var result = command.ExecuteScalar();

                if (result == null || Convert.ToInt32(result) < item.Quantity)
                {
                    hasStock = false;
                    break;
                }
            }

            if (!hasStock)
            {
                var failed = new InventoryFailed
                {
                    OrderId = order.OrderId,
                    FailedAtUtc = DateTime.UtcNow,
                    Message = "Not enough stock."
                };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(failed));

                channel.BasicPublish("", "inventory-failed", null, body);
                return;
            }

            foreach (var item in order.Items)
            {
                var update = db.CreateCommand();
                update.CommandText =
                @"
                UPDATE Products 
                SET StockQuantity = StockQuantity - $qty 
                WHERE Id = $id
                ";
                update.Parameters.AddWithValue("$qty", item.Quantity);
                update.Parameters.AddWithValue("$id", item.ProductId);
                update.ExecuteNonQuery();
            }

            var confirmed = new InventoryConfirmed
            {
                OrderId = order.OrderId,
                ConfirmedAtUtc = DateTime.UtcNow,
                Message = "Inventory confirmed successfully."
            };

            var confirmedBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(confirmed));

            channel.BasicPublish("", "inventory-confirmed", null, confirmedBody);
        };

        channel.BasicConsume("order-submitted", true, consumer);

        return Task.CompletedTask;
    }
}