using DeliveryMicroservice.Domain.Entities;
using DeliveryMicroservice.Domain.Interfaces;
using DeliveryMicroservice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryMicroservice.Infrastructure.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly DeliveryDbContext _context;

    public DeliveryRepository(DeliveryDbContext context)
    {
        _context = context;
    }

    public async Task<Delivery?> GetByIdAsync(Guid id)
    {
        return await _context.Deliveries.FindAsync(id);
    }

    public async Task<IEnumerable<Delivery>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Deliveries
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Delivery> CreateAsync(Delivery delivery)
    {
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();
        return delivery;
    }

    public async Task<Delivery> UpdateAsync(Delivery delivery)
    {
        _context.Entry(delivery).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return delivery;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var delivery = await _context.Deliveries.FindAsync(id);
        if (delivery == null)
            return false;

        _context.Deliveries.Remove(delivery);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Delivery?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Deliveries
            .FirstOrDefaultAsync(d => d.OrderId == orderId);
    }
} 