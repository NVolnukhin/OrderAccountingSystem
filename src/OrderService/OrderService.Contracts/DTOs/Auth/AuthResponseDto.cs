namespace OrderService.Contracts.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    string Role
); 