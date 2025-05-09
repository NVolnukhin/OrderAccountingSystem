namespace UserMicroservice.API.DTOs;

public record UserProfileResponse(
    Guid Id,
    string Email,
    string? Phone,
    string? FullName,
    DateTime CreatedAt
);

public record UpdateUserProfileRequest(
    string? Phone,
    string? FullName
);

public record UserAddressResponse(
    int Id,
    string? Label,
    string? Country,
    string? City,
    string? Street,
    string? House,
    string? Apartment,
    string? PostalCode,
    bool IsDefault
);

public record CreateUserAddressRequest(
    string? Label,
    string? Country,
    string? City,
    string? Street,
    string? House,
    string? Apartment,
    string? PostalCode,
    bool IsDefault
);

public record UpdateUserAddressRequest(
    string? Label,
    string? Country,
    string? City,
    string? Street,
    string? House,
    string? Apartment,
    string? PostalCode,
    bool IsDefault
); 