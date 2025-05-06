namespace OrderService.Contracts.DTOs.Auth;

public record RegisterDto(
    string Username,
    string Email,
    string Password
); 