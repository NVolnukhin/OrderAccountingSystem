using System.Text.Json.Serialization;

namespace CartMicroservice.Contracts.Messages;

public class CartCheckoutMessage
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("items")]
    public List<CartItemMessage> Items { get; set; } = new();

    [JsonPropertyName("deliveryAddress")]
    public string DeliveryAddress { get; set; } = string.Empty;
}

public class CartItemMessage
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
} 