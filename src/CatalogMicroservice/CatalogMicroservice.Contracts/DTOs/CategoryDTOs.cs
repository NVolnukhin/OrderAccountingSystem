namespace CatalogMicroservice.Contracts.DTOs;

public record CategoryResponse(
    int Id,
    string Name,
    int? ParentId
); 