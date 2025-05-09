namespace CatalogMicroservice.Contracts.DTOs;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int? CategoryId,
    string? ImageUrl,
    IEnumerable<ProductAttributeResponse> Attributes
);

public record ProductAttributeResponse(
    string Key,
    string Value
);

public record GetProductsByIdsRequest(
    IEnumerable<int> Ids
); 