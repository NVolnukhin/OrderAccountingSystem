namespace CatalogMicroservice.Domain.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
} 