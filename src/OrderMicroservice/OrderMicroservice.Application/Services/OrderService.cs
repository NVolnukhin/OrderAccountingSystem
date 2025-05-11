using Microsoft.Extensions.Logging;
using OrderMicroservice.Application.Interfaces;
using OrderMicroservice.Contracts.DTOs.Order;
using OrderMicroservice.Domain.Entities;

namespace OrderMicroservice.Application.Services;

public enum OrderStatus
{
    Created,
    Pending,
    Paid,
    Unpaid,
    Refunded,
    TransferredForDelivery,
    Delivered,
    Cancelled
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMessageBroker _messageBroker;
    private readonly ICatalogService _catalogService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IMessageBroker messageBroker,
        ICatalogService catalogService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _messageBroker = messageBroker;
        _catalogService = catalogService;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto, Guid userId)
    {
        // Get product information from catalog service
        var productIds = createOrderDto.Items.Select(i => i.ProductId).ToList();
        var products = await _catalogService.GetProductsInfoAsync(productIds);

        if (!products.Any())
        {
            _logger.LogWarning("Products not found in catalog: {ProductIds}", string.Join(", ", productIds));
            throw new KeyNotFoundException($"Products not found in catalog: {string.Join(", ", productIds)}");
        }

        var productsDict = products.ToDictionary(p => p.Id);

        // Check if all products exist and have sufficient stock
        foreach (var item in createOrderDto.Items)
        {
            if (!productsDict.TryGetValue(item.ProductId, out var product))
            {
                _logger.LogWarning("Product {ProductId} not found in catalog", item.ProductId);
                throw new KeyNotFoundException($"Product {item.ProductId} not found in catalog");
            }

            if (item.Quantity > product.StockQuantity)
            {
                _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}", 
                    item.ProductId, item.Quantity, product.StockQuantity);
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. Requested: {item.Quantity}, Available: {product.StockQuantity}");
            }
        }

        // Create order
        var order = new Order
        {
            UserId = userId,
            DeliveryAddress = createOrderDto.DeliveryAddress,
            Status = OrderStatus.Pending.ToString(),
            Items = createOrderDto.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = productsDict[i.ProductId].Name,
                ProductPrice = productsDict[i.ProductId].Price,
                Quantity = i.Quantity
            }).ToList()
        };

        // Calculate total price
        order.TotalPrice = order.Items.Sum(i => i.ProductPrice * i.Quantity);

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        var orderResponse = MapToOrderResponse(order);
        await _messageBroker.PublishOrderCreatedAsync(orderResponse);

        return orderResponse;
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }
        return MapToOrderResponse(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(Guid userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToOrderResponse);
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid id, string status)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        var orderResponse = MapToOrderResponse(order);
        await _messageBroker.PublishOrderStatusChangedAsync(id, status);

        return orderResponse;
    }

    private static OrderResponseDto MapToOrderResponse(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            DeliveryAddress = order.DeliveryAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductPrice = i.ProductPrice,
                Quantity = i.Quantity
            }).ToList()
        };
    }
} 