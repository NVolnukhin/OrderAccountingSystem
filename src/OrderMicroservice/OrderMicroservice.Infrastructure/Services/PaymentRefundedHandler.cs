using Microsoft.Extensions.Logging;
using OrderMicroservice.Application.Interfaces;
using OrderMicroservice.Application.Services;
using Shared.Contracts;
using Shared.Contracts.Events;

namespace OrderMicroservice.Infrastructure.Services;

public class PaymentRefundedHandler : IMessageHandler<PaymentRefundedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentRefundedHandler> _logger;

    public PaymentRefundedHandler(
        IOrderService orderService,
        ILogger<PaymentRefundedHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentRefundedEvent message)
    {
        try
        {
            _logger.LogInformation("Received payment refunded event for order {OrderId} with payment {PaymentId}", 
                message.OrderId, message.PaymentId);
            
            _logger.LogInformation("Current order status before update: {OrderId}", message.OrderId);
            var currentOrder = await _orderService.GetOrderByIdAsync(message.OrderId);
            _logger.LogInformation("Current order status: {Status}", currentOrder.Status);
            
            _logger.LogInformation("Updating order {OrderId} status to Cancelled", message.OrderId);
            await _orderService.UpdateOrderStatusAsync(message.OrderId, OrderStatus.Refunded.ToString());
            
            var updatedOrder = await _orderService.GetOrderByIdAsync(message.OrderId);
            _logger.LogInformation("Order {OrderId} status updated to {Status}", 
                message.OrderId, updatedOrder.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment refunded event for order {OrderId}: {ErrorMessage}", 
                message.OrderId, ex.Message);
            throw;
        }
    }
} 