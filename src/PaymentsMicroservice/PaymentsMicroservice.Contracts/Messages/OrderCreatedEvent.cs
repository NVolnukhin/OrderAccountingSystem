using System;
using System.Text.Json.Serialization;

namespace PaymentsMicroservice.Contracts.Messages;

public class OrderCreatedEvent
{
    [JsonPropertyName("Id")]
    public Guid OrderId { get; set; }

    [JsonPropertyName("TotalPrice")]
    public decimal Amount { get; set; }
} 