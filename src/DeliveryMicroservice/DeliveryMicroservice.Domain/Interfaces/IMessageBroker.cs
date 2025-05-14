namespace DeliveryMicroservice.Domain.Interfaces;

public interface IMessageBroker
{
    Task PublishAsync<T>(T message) where T : class;
    Task SubscribeToOrderEventsAsync();
    Task SubscribeToPaymentEventsAsync();
} 