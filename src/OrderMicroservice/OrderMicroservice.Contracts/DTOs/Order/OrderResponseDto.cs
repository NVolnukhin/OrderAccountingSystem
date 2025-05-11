namespace OrderMicroservice.Contracts.DTOs.Order;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public string DeliveryAddress { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
} 