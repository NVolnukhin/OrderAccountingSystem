using CatalogMicroservice.Domain.Models;

namespace CatalogMicroservice.Domain.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<int> ids);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task SaveChangesAsync();
} 