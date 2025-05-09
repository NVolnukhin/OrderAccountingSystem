namespace CatalogMicroservice.Domain.Models;

public class ProductAttribute
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
} 