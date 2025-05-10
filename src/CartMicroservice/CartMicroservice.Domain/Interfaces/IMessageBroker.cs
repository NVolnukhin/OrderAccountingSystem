using CartMicroservice.Contracts.Messages;

namespace CartMicroservice.Domain.Interfaces;

public interface IMessageBroker
{
    Task PublishCartCheckoutAsync(CartCheckoutMessage message);
} 