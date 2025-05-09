namespace UserMicroservice.API.DTOs;

public record CreateUserFromAuthRequest(
    Guid Id,
    string Email
); 