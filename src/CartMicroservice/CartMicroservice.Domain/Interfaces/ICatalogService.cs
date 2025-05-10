namespace CartMicroservice.Domain.Interfaces;

public interface ICatalogService
{
    Task<CatalogProductInfo?> GetProductInfoAsync(int productId);
    Task<IEnumerable<CatalogProductInfo>> GetProductsInfoAsync(IEnumerable<int> productIds);
}

public record CatalogProductInfo(
    int Id,
    string Name,
    decimal Price,
    string? ImageUrl,
    Dictionary<string, string> Attributes,
    int StockQuantity
); 