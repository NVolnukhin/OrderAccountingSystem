namespace OrderMicroservice.Contracts.DTOs.Order;

public record OrderWithUserInfoDto(
    int Id,
    string Username,
    string Email,
    string ShippingAddress,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    List<OrderItemResponseDto> Items
); 