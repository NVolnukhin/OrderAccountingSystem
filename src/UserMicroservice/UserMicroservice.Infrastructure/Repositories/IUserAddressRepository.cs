using UserMicroservice.Domain.Models;

namespace UserMicroservice.Domain.Interfaces;

public interface IUserAddressRepository
{
    Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId);
    Task<UserAddress?> GetByIdAsync(int id);
    Task AddAsync(UserAddress address);
    Task UpdateAsync(UserAddress address);
    Task DeleteAsync(UserAddress address);
    Task SaveChangesAsync();
} 