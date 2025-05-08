namespace AuthMicroservice.Domain.Models;

public class AuthUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<PasswordResetToken> PasswordResetTokens { get; set; } = new();
} 