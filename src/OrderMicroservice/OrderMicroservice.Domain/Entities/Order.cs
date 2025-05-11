using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderMicroservice.Domain.Entities;

public class Order
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string Status { get; set; } = null!;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }
    
    public string DeliveryAddress { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
} 