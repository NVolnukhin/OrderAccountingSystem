namespace AuthMicroservice.Contracts.DTOs;

public record ResetPasswordRequest(
    string Token,
    string NewPassword
); 