using DeliveryMicroservice.Domain.Entities;

namespace DeliveryMicroservice.Application.Interfaces;

public interface IDeliveryService
{
    Task<Delivery?> GetDeliveryByIdAsync(Guid id);
    Task<IEnumerable<Delivery>> GetDeliveriesByUserIdAsync(Guid userId);
    Task<Delivery> CreateDeliveryAsync(Delivery delivery);
    Task<Delivery> UpdateDeliveryStatusAsync(Guid id, string status);
    Task<bool> DeleteDeliveryAsync(Guid id);
    Task<Delivery?> GetDeliveryByOrderIdAsync(Guid orderId);
} 