using System;
using System.Threading.Tasks;
using CartMicroservice.Domain.Entities;

namespace CartMicroservice.Domain.Interfaces;

public interface ICartItemRepository
{
    Task<CartItem?> GetByIdAsync(Guid id);
    Task<CartItem?> GetByCartAndProductAsync(Guid cartId, int productId);
    Task<CartItem> CreateAsync(CartItem item);
    Task UpdateAsync(CartItem item);
    Task DeleteAsync(Guid id);
} 