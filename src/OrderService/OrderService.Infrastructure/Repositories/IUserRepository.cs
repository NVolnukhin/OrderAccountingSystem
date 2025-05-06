using OrderService.Domain.Models;

namespace OrderService.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
} 