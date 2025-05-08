namespace AuthMicroservice.Contracts.DTOs;

public record RegisterRequest(
    string Email,
    string Password
); 