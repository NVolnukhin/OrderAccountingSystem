namespace OrderService.Contracts.DTOs.Auth;

public record LoginDto(
    string Email,
    string Password
); 