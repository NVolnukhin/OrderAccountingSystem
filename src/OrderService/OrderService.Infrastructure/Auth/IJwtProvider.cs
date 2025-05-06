using OrderService.Domain.Models;

namespace OrderService.Infrastructure.Auth;

public interface IJwtProvider
{
    string GenerateToken(User user);
} 