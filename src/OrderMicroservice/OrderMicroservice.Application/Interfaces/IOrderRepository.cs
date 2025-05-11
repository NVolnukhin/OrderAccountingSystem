using OrderMicroservice.Domain.Entities;

namespace OrderMicroservice.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Order>> GetAllAsync();
    Task AddAsync(Order order);
    void Update(Order order);
    Task SaveChangesAsync();
} 