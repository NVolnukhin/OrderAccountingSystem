using AuthMicroservice.Domain.Interfaces;
using AuthMicroservice.Domain.Models;
using AuthMicroservice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AuthDbContext _context;

    public PasswordResetTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
    }

    public Task UpdateAsync(PasswordResetToken token)
    {
        _context.Entry(token).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
} 