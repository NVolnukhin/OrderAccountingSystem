using System.Threading.Tasks;
using OrderMicroservice.Contracts.DTOs.Order;
using Shared.Contracts;

namespace OrderMicroservice.Application.Interfaces;

public interface IMessageBroker
{
    Task PublishOrderCreatedAsync(OrderResponseDto order);
    Task PublishOrderStatusChangedAsync(Guid orderId, string status);
    Task SubscribeToPaymentEventsAsync();
    Task SubscribeToDeliveryEventsAsync();
    Task SubscribeAsync<T>(IMessageHandler<T> handler) where T : class;
} 