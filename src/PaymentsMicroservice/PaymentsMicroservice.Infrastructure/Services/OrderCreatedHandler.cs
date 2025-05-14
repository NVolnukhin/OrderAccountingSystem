using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Application.Interfaces;
using Shared.Contracts.Events;

namespace PaymentsMicroservice.Infrastructure.Services;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(
        IPaymentService paymentService,
        ILogger<OrderCreatedHandler> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent message)
    {
        _logger.LogInformation("Received order.created event for order {OrderId} with amount {Amount}", 
            message.OrderId, message.Amount);

        // Check if payment already exists
        var existingPayment = await _paymentService.GetPaymentByOrderIdAsync(message.OrderId);
        if (existingPayment != null)
        {
            _logger.LogWarning("Payment already exists for order {OrderId} with payment ID {PaymentId}", 
                message.OrderId, existingPayment.Id);
            return;
        }

        try
        {
            var payment = await _paymentService.CreatePaymentAsync(message.OrderId, message.Amount);
            _logger.LogInformation("Created payment {PaymentId} for order {OrderId}", payment.Id, message.OrderId);

            await _paymentService.ProcessPaymentAsync(payment.Id);
            _logger.LogInformation("Processed payment {PaymentId} for order {OrderId}", payment.Id, message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", message.OrderId);
            throw;
        }
    }
} 