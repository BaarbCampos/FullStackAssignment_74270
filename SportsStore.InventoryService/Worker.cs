using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

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

        channel.QueueDeclare(
            queue: "order-submitted",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine("📦 Order received:");
            Console.WriteLine(message);
        };

        channel.BasicConsume(
            queue: "order-submitted",
            autoAck: true,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}