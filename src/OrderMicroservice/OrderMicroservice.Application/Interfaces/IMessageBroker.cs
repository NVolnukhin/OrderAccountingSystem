using OrderMicroservice.Contracts.DTOs.Order;

namespace OrderMicroservice.Application.Interfaces;

public interface IMessageBroker
{
    Task PublishOrderCreatedAsync(OrderResponseDto order);
    Task PublishOrderStatusChangedAsync(Guid orderId, string status);
} 