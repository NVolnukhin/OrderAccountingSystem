using AuthMicroservice.Domain.Interfaces;
using AuthMicroservice.Domain.Models;
using AuthMicroservice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Repositories;

public class AuthUserRepository : IAuthUserRepository
{
    private readonly AuthDbContext _context;

    public AuthUserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<AuthUser?> GetByIdAsync(Guid id)
    {
        return await _context.AuthUsers.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AuthUser?> GetByEmailAsync(string email)
    {
        return await _context.AuthUsers.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.AuthUsers.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(AuthUser user)
    {
        await _context.AuthUsers.AddAsync(user);
    }

    public Task UpdateAsync(AuthUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AuthUser user)
    {
        _context.AuthUsers.Remove(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
} 