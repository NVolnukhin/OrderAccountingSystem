using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Application.Interfaces;
using PaymentsMicroservice.Domain.Entities;
using PaymentsMicroservice.Infrastructure.Data;
using Shared.Contracts.Events;

namespace PaymentsMicroservice.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentsDbContext _context;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<PaymentService> _logger;
    private readonly Random _random;

    public PaymentService(
        PaymentsDbContext context,
        IMessageBroker messageBroker,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _messageBroker = messageBroker;
        _logger = logger;
        _random = new Random();
    }

    public async Task<Payment> CreatePaymentAsync(Guid orderId, decimal amount)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created payment {PaymentId} for order {OrderId}", payment.Id, orderId);
        return payment;
    }

    public async Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<Payment> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
        {
            throw new ArgumentException($"Payment {paymentId} not found");
        }
        return payment;
    }

    public async Task<Payment> ProcessPaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId);
            
        if (payment == null)
        {
            _logger.LogError("Payment {PaymentId} not found", paymentId);
            throw new ArgumentException($"Payment {paymentId} not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            _logger.LogWarning("Payment {PaymentId} is already processed with status {Status}", 
                paymentId, payment.Status);
            return payment;
        }

        _logger.LogInformation("Processing payment {PaymentId} for order {OrderId}", 
            paymentId, payment.OrderId);

        // Эмуляция обработки платежа
        await Task.Delay(_random.Next(8000, 10000));

        // 90% шанс успешной оплаты
        if (_random.Next(100) < 90)
        {
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _messageBroker.PublishAsync(new PaymentCompletedEvent
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                CompletedAt = payment.CompletedAt.Value
            });

            _logger.LogInformation("Payment {PaymentId} completed successfully", paymentId);
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.CompletedAt = DateTime.UtcNow;
            payment.ErrorMessage = "Payment processing failed";
            await _context.SaveChangesAsync();

            await _messageBroker.PublishAsync(new PaymentFailedEvent
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                FailedAt = payment.CompletedAt.Value,
                ErrorMessage = payment.ErrorMessage
            });

            _logger.LogWarning("Payment {PaymentId} failed", paymentId);
        }

        return payment;
    }

    public async Task<Payment> RefundPaymentAsync(Guid paymentId)
    {
        var payment = await GetPaymentByIdAsync(paymentId);
        
        if (payment.Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot refund payment with status {payment.Status}");
        }

        payment.Status = PaymentStatus.Refunded;
        await _context.SaveChangesAsync();

        await _messageBroker.PublishAsync(new PaymentRefundedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            RefundedAt = DateTime.UtcNow
        });

        return payment;
    }

    public async Task<Payment> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus)
    {
        var payment = await GetPaymentByIdAsync(paymentId);
        
        if (payment.Status == newStatus)
        {
            return payment;
        }

        payment.Status = newStatus;
        
        switch (newStatus)
        {
            case PaymentStatus.Completed:
                payment.CompletedAt = DateTime.UtcNow;
                await _messageBroker.PublishAsync(new PaymentCompletedEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    CompletedAt = payment.CompletedAt.Value
                });
                break;
            
            case PaymentStatus.Failed:
                payment.FailedAt = DateTime.UtcNow;
                await _messageBroker.PublishAsync(new PaymentFailedEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    FailedAt = payment.FailedAt.Value,
                    ErrorMessage = "Payment failed"
                });
                break;
        }

        await _context.SaveChangesAsync();
        return payment;
    }
} 