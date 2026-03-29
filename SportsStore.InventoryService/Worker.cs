using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace SportsStore.InventoryService;

public class Worker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

        // Filas
        channel.QueueDeclare("order-submitted", false, false, false);
        channel.QueueDeclare("inventory-confirmed", false, false, false);
        channel.QueueDeclare("inventory-failed", false, false, false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var order = JsonSerializer.Deserialize<OrderSubmitted>(json);

            if (order == null) return;

            Console.WriteLine("📦 Order received:");
            Console.WriteLine(json);

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

            // ❌ SEM STOCK
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

                Console.WriteLine("❌ Inventory failed!");
                return;
            }

            // ✅ TEM STOCK → Atualiza BD
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

            Console.WriteLine("✅ Inventory confirmed!");
        };

        channel.BasicConsume("order-submitted", true, consumer);

        return Task.CompletedTask;
    }
}