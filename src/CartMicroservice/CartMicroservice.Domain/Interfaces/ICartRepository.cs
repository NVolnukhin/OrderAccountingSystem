using System;
using System.Threading.Tasks;
using CartMicroservice.Domain.Entities;

namespace CartMicroservice.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id);
    Task<Cart?> GetByUserIdAsync(Guid userId);
    Task<Cart?> GetBySessionTokenAsync(string sessionToken);
    Task<Cart> CreateAsync(Cart cart);
    Task UpdateAsync(Cart cart);
    Task DeleteAsync(Guid id);
} 