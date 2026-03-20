namespace SportsStore.OrderApi.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message);
}