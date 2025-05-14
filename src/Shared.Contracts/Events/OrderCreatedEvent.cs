using System;

namespace Shared.Contracts.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public decimal Amount { get; set; }
} 