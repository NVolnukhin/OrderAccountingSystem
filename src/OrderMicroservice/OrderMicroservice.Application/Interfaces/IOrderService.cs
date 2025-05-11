using OrderMicroservice.Contracts.DTOs.Order;

namespace OrderMicroservice.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto, Guid userId);
    Task<OrderResponseDto> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid id, string status);
} 