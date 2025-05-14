namespace Shared.Contracts.Events;

public class PaymentCompletedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CompletedAt { get; set; }
}
