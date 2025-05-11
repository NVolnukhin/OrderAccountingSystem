namespace OrderMicroservice.Contracts.DTOs.Order;

public record OrderItemResponseDto(
    Guid Id,
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
); 