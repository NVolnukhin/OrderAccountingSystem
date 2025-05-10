namespace CartMicroservice.Domain.Models;

public class Cart
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
} 