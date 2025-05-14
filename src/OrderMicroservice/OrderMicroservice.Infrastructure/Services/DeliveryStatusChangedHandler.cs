using Microsoft.Extensions.Logging;
using OrderMicroservice.Application.Interfaces;
using OrderMicroservice.Application.Services;
using Shared.Contracts;
using Shared.Contracts.Events;

namespace OrderMicroservice.Infrastructure.Services;

public class DeliveryStatusChangedHandler : IMessageHandler<DeliveryStatusUpdatedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<DeliveryStatusChangedHandler> _logger;

    public DeliveryStatusChangedHandler(
        IOrderService orderService,
        ILogger<DeliveryStatusChangedHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task HandleAsync(DeliveryStatusUpdatedEvent message)
    {
        try
        {
            _logger.LogInformation("Received delivery status changed event for order {OrderId} with status {Status}", 
                message.OrderId, message.Status);
            
            var currentOrder = await _orderService.GetOrderByIdAsync(message.OrderId);
            if (currentOrder == null)
            {
                _logger.LogWarning("Order {OrderId} not found, skipping status update", message.OrderId);
                return;
            }
            
            _logger.LogInformation("Current order status: {Status}", currentOrder.Status);
            
            // Маппинг статусов доставки на статусы заказа
            string orderStatus = message.Status switch
            {
                "Preparing" => OrderStatus.PreparingForDelivery.ToString(),
                "Shipped" => OrderStatus.Shipped.ToString(),
                "Delivered" => OrderStatus.Delivered.ToString(),
                "Canceled" => OrderStatus.Cancelled.ToString(),
                _ => currentOrder.Status // Если статус неизвестен, оставляем текущий
            };
            
            _logger.LogInformation("Updating order {OrderId} status to {Status}", message.OrderId, orderStatus);
            await _orderService.UpdateOrderStatusAsync(message.OrderId, orderStatus);
            
            var updatedOrder = await _orderService.GetOrderByIdAsync(message.OrderId);
            _logger.LogInformation("Order {OrderId} status updated to {Status}", 
                message.OrderId, updatedOrder.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling delivery status changed event for order {OrderId}: {ErrorMessage}", 
                message.OrderId, ex.Message);
            throw;
        }
    }
} 