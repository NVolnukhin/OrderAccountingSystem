using AuthMicroservice.Domain.Models;

namespace AuthMicroservice.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task AddAsync(PasswordResetToken token);
    Task UpdateAsync(PasswordResetToken token);
    Task SaveChangesAsync();
} 