using AuthMicroservice.Domain.Models;

namespace AuthMicroservice.Domain.Interfaces;

public interface ITokenService
{
    Task<PasswordResetToken> CreatePasswordResetTokenAsync(Guid userId);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token);
    Task InvalidatePasswordResetTokenAsync(string token);
    Task<bool> ValidatePasswordResetTokenAsync(string token);
} 