namespace Shared.Contracts.Events;

public class DeliveryStatusUpdatedEvent
{
    public Guid DeliveryId { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
} 