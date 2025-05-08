using System.Security.Cryptography;
using AuthMicroservice.Domain.Interfaces;
using AuthMicroservice.Domain.Models;

namespace AuthMicroservice.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IAuthUserRepository _userRepository;

    public TokenService(
        IPasswordResetTokenRepository tokenRepository,
        IAuthUserRepository userRepository)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
    }

    public async Task<PasswordResetToken> CreatePasswordResetTokenAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        await _tokenRepository.AddAsync(token);
        await _tokenRepository.SaveChangesAsync();

        return token;
    }

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token)
    {
        return await _tokenRepository.GetByTokenAsync(token);
    }

    public async Task InvalidatePasswordResetTokenAsync(string token)
    {
        var resetToken = await _tokenRepository.GetByTokenAsync(token);
        if (resetToken != null)
        {
            resetToken.IsUsed = true;
            await _tokenRepository.UpdateAsync(resetToken);
            await _tokenRepository.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string token)
    {
        var resetToken = await _tokenRepository.GetByTokenAsync(token);
        if (resetToken == null)
            return false;

        return !resetToken.IsUsed && resetToken.ExpiresAt > DateTime.UtcNow;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
} 