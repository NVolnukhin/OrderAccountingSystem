using System;
using System.Collections.Generic;

namespace CartMicroservice.Domain.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
} 