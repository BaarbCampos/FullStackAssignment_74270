using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SportsStore.OrderApi.Configuration;

namespace SportsStore.OrderApi.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqMessagePublisher(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;
    }

    public Task PublishAsync<T>(string queueName, T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body);

        Console.WriteLine($"[RABBITMQ] Message sent to queue '{queueName}'");

        return Task.CompletedTask;
    }
}