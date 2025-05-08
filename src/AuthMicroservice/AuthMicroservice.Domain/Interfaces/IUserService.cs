using AuthMicroservice.Domain.Models;

namespace AuthMicroservice.Domain.Interfaces;

public interface IUserService
{
    Task<AuthUser?> GetByIdAsync(Guid id);
    Task<AuthUser?> GetByEmailAsync(string email);
    Task<AuthUser> CreateAsync(string email, string password);
    Task UpdateAsync(AuthUser user);
    Task DeleteAsync(Guid id);
    Task<bool> ValidateCredentialsAsync(string email, string password);
} 