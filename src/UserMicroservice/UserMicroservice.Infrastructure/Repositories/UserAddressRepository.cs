using Microsoft.EntityFrameworkCore;
using UserMicroservice.Domain.Interfaces;
using UserMicroservice.Domain.Models;
using UserMicroservice.Infrastructure.Data;

namespace UserMicroservice.Infrastructure.Repositories;

public class UserAddressRepository : IUserAddressRepository
{
    private readonly UserDbContext _context;

    public UserAddressRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserAddress?> GetByIdAsync(int id)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(UserAddress address)
    {
        await _context.UserAddresses.AddAsync(address);
    }

    public Task UpdateAsync(UserAddress address)
    {
        _context.Entry(address).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserAddress address)
    {
        _context.UserAddresses.Remove(address);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
} 