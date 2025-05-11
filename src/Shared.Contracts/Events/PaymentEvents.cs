using System;

namespace Shared.Contracts.Events;

public class PaymentCompletedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PaymentRefundedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
} 