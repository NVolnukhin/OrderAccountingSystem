using System.Threading.Tasks;

namespace PaymentsMicroservice.Application.Interfaces;

public interface IMessageBroker
{
    Task PublishAsync<T>(T message);
    Task SubscribeAsync<T>(string queueName, string routingKey, IMessageHandler<T> handler);
}

public interface IMessageHandler<T>
{
    Task HandleAsync(T message);
} 