using CartMicroservice.Domain.Entities;
using CartMicroservice.Domain.Interfaces;
using CartMicroservice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CartMicroservice.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;

    public CartRepository(CartDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByIdAsync(Guid id)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetBySessionTokenAsync(string sessionToken)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionToken == sessionToken);
    }

    public async Task<Cart> CreateAsync(Cart cart)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task UpdateAsync(Cart cart)
    {
        _context.Entry(cart).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart != null)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
} 