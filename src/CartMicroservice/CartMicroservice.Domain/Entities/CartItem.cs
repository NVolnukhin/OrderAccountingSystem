using System;
using System.Text.Json.Serialization;

namespace CartMicroservice.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    
    [JsonIgnore]
    public Cart Cart { get; set; } = null!;
} 