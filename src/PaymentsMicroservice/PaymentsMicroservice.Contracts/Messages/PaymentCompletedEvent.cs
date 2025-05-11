using System;

namespace PaymentsMicroservice.Contracts.Messages;

public class PaymentCompletedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CompletedAt { get; set; }
} 