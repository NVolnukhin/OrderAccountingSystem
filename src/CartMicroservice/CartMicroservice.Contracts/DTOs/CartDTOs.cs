namespace CartMicroservice.Contracts.DTOs;

public record CartResponse(
    Guid Id,
    Guid? UserId,
    string? SessionToken,
    DateTime CreatedAt,
    IEnumerable<CartItemResponse> Items
);

public record CartItemResponse(
    int Id,
    int ProductId,
    int Quantity,
    ProductInfo Product
);

public record ProductInfo(
    int Id,
    string Name,
    decimal Price,
    string? ImageUrl,
    Dictionary<string, string> Attributes
);

public record AddToCartRequest(
    int ProductId,
    int Quantity
);

public record UpdateCartItemRequest(
    int Quantity
); 