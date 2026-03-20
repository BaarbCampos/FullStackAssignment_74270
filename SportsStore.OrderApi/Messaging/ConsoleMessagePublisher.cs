using System.Text.Json;

namespace SportsStore.OrderApi.Messaging;

public class ConsoleMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<T>(string queueName, T message)
    {
        var json = JsonSerializer.Serialize(message);
        Console.WriteLine($"[MESSAGE PUBLISHED] Queue: {queueName}");
        Console.WriteLine(json);

        return Task.CompletedTask;
    }
}