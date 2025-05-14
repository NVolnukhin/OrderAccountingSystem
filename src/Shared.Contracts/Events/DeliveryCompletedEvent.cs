namespace Shared.Contracts.Events;

public class DeliveryCompletedEvent
{
    public Guid DeliveryId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}
