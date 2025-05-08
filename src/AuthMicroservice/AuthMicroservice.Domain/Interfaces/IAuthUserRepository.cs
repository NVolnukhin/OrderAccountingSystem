using AuthMicroservice.Domain.Models;

namespace AuthMicroservice.Domain.Interfaces;

public interface IAuthUserRepository
{
    Task<AuthUser?> GetByIdAsync(Guid id);
    Task<AuthUser?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(AuthUser user);
    Task UpdateAsync(AuthUser user);
    Task DeleteAsync(AuthUser user);
    Task SaveChangesAsync();
} 