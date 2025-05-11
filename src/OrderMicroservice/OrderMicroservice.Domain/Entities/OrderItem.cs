using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderMicroservice.Domain.Entities;

public class OrderItem
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrderId { get; set; }
    
    public int ProductId { get; set; }
    
    public string ProductName { get; set; } = null!;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal ProductPrice { get; set; }
    
    public int Quantity { get; set; }
    
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
} 