namespace CartMicroservice.Domain.Models;

public class CartItem
{
    public int Id { get; set; }
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
} 