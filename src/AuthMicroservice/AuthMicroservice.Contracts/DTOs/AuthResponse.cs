namespace AuthMicroservice.Contracts.DTOs;

public record AuthResponse(
    string Token,
    Guid UserId
); 