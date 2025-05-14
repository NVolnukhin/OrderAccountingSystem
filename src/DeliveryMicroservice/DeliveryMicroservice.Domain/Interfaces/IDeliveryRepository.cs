using DeliveryMicroservice.Domain.Entities;

namespace DeliveryMicroservice.Domain.Interfaces;

public interface IDeliveryRepository
{
    Task<Delivery?> GetByIdAsync(Guid id);
    Task<IEnumerable<Delivery>> GetByUserIdAsync(Guid userId);
    Task<Delivery> CreateAsync(Delivery delivery);
    Task<Delivery> UpdateAsync(Delivery delivery);
    Task<bool> DeleteAsync(Guid id);
    Task<Delivery?> GetByOrderIdAsync(Guid orderId);
} 