namespace AuthMicroservice.Domain.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(Guid userId);
    bool ValidateToken(string token, out Guid userId);
} 