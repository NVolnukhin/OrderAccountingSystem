namespace Shared.Contracts.Events;

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime FailedAt { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
