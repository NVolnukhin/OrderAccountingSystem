using System;

namespace PaymentsMicroservice.Contracts.Messages;

public class PaymentFailedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime FailedAt { get; set; }
    public string ErrorMessage { get; set; }
} 