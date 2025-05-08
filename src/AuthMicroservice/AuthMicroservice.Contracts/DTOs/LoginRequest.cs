namespace AuthMicroservice.Contracts.DTOs;

public record LoginRequest(
    string Email,
    string Password
); 