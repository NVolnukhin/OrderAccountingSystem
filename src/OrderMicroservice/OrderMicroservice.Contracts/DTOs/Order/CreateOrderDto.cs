namespace OrderMicroservice.Contracts.DTOs.Order;

public class CreateOrderDto
{
    public Guid UserId { get; set; }
    public string DeliveryAddress { get; set; } = null!;
    public ICollection<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
} 