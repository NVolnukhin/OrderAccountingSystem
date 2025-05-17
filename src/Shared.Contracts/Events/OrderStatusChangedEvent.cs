namespace Shared.Contracts.Events;

public class OrderStatusChangedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
} 