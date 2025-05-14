namespace Shared.Contracts.Events;

public class PaymentRefundedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RefundedAt { get; set; }
}