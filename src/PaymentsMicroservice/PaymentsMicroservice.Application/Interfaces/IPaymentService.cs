using System;
using System.Threading.Tasks;
using PaymentsMicroservice.Domain.Entities;

namespace PaymentsMicroservice.Application.Interfaces;

public interface IPaymentService
{
    Task<Payment> CreatePaymentAsync(Guid orderId, decimal amount);
    Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId);
    Task<Payment> ProcessPaymentAsync(Guid paymentId);
    Task<Payment> RefundPaymentAsync(Guid paymentId);
    Task<Payment> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus);
} 