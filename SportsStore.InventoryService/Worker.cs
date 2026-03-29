using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Shared.Contracts;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace SportsStore.InventoryService;

public class Worker : BackgroundService
{
    private const string connectionString = "Data Source=../SportsStore.OrderApi/orders.db";

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

        channel.QueueDeclare("order-submitted", false, false, false, null);
        channel.QueueDeclare("inventory-confirmed", false, false, false, null);
        channel.QueueDeclare("inventory-failed", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var order = JsonSerializer.Deserialize<OrderSubmitted>(json);

            if (order == null) return;

            using var db = new SqliteConnection(connectionString);
            db.Open();

            bool hasStock = true;

            foreach (var item in order.Items)
            {
                var cmd = db.CreateCommand();
                cmd.CommandText = "SELECT StockQuantity FROM Products WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", item.ProductId);

                var stock = Convert.ToInt32(cmd.ExecuteScalar());

                if (stock < item.Quantity)
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
                    Reason = "Not enough stock"
                };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(failed));

                channel.BasicPublish("", "inventory-failed", null, body);

                Console.WriteLine("❌ Inventory failed");
                return;
            }

            // ✅ Deduz stock
            foreach (var item in order.Items)
            {
                var cmd = db.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Products 
                    SET StockQuantity = StockQuantity - $qty 
                    WHERE Id = $id";

                cmd.Parameters.AddWithValue("$qty", item.Quantity);
                cmd.Parameters.AddWithValue("$id", item.ProductId);

                cmd.ExecuteNonQuery();
            }

            var confirmed = new InventoryConfirmed
            {
                OrderId = order.OrderId,
                ConfirmedAtUtc = DateTime.UtcNow,
                Message = "Stock reserved"
            };

            var confirmedBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(confirmed));

            channel.BasicPublish("", "inventory-confirmed", null, confirmedBody);

            Console.WriteLine("✅ Inventory confirmed");
        };

        channel.BasicConsume("order-submitted", true, consumer);

        return Task.CompletedTask;
    }
}