using AuthMicroservice.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace AuthMicroservice.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHashingSettings _settings;

    public PasswordHasher(IOptions<PasswordHashingSettings> settings)
    {
        _settings = settings.Value;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, _settings.WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
} 