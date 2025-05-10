using CartMicroservice.Domain.Entities;
using CartMicroservice.Domain.Interfaces;
using CartMicroservice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CartMicroservice.Infrastructure.Repositories;

public class CartItemRepository : ICartItemRepository
{
    private readonly CartDbContext _context;

    public CartItemRepository(CartDbContext context)
    {
        _context = context;
    }

    public async Task<CartItem?> GetByIdAsync(Guid id)
    {
        return await _context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<CartItem?> GetByCartAndProductAsync(Guid cartId, int productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);
    }

    public async Task<CartItem> CreateAsync(CartItem item)
    {
        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(CartItem item)
    {
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
} 