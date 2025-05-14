using System.ComponentModel.DataAnnotations;

namespace DeliveryMicroservice.Domain.Entities;

public class Delivery
{
    [Key]
    public Guid DeliveryId { get; set; }
    
    public Guid OrderId { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    public string Status { get; set; } = string.Empty;
    
    [Required]
    public string TrackingNumber { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public static class DeliveryStatus
{
    public const string Pending = "Pending";
    public const string Preparing = "Preparing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Canceled = "Canceled";
} 