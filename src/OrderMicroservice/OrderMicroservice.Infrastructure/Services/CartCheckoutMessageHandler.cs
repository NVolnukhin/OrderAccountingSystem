using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderMicroservice.Application.Interfaces;
using OrderMicroservice.Contracts.DTOs.Order;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderMicroservice.Infrastructure.Services;

public class CartCheckoutMessageHandler : IDisposable
{
    private const string CartCheckoutQueue = "cart.checkout";
    private const string CartCheckoutErrorQueue = "cart.checkout.error";
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CartCheckoutMessageHandler> _logger;
    private bool _disposed;

    public CartCheckoutMessageHandler(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<CartCheckoutMessageHandler> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;

        _channel = _connection.CreateModel();
        
        // Declare main queue
        _channel.QueueDeclare(CartCheckoutQueue, true, false, false);
        
        // Declare error queue
        _channel.QueueDeclare(CartCheckoutErrorQueue, true, false, false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += HandleMessage;
        _channel.BasicConsume(CartCheckoutQueue, false, consumer);
    }

    private void HandleMessage(object? model, BasicDeliverEventArgs ea)
    {
        if (_disposed)
        {
            _logger.LogWarning("Message handler is disposed, skipping message processing");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var body = ea.Body.ToArray();
        var message = System.Text.Encoding.UTF8.GetString(body);

        try
        {
            var checkoutMessage = JsonSerializer.Deserialize<CartCheckoutMessage>(message);
            if (checkoutMessage == null)
            {
                _logger.LogError("Failed to deserialize cart checkout message");
                _channel.BasicNack(ea.DeliveryTag, false, false);
                return;
            }

            var catalogService = scope.ServiceProvider.GetRequiredService<ICatalogService>();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            // Проверяем наличие всех товаров
            var productIds = checkoutMessage.Items.Select(i => i.ProductId).ToList();
            var products = catalogService.GetProductsInfoAsync(productIds).GetAwaiter().GetResult();
            var productsDict = products.ToDictionary(p => p.Id);

            var insufficientStockItems = new List<(int ProductId, string Name, int Requested, int Available)>();
            foreach (var item in checkoutMessage.Items)
            {
                if (!productsDict.TryGetValue(item.ProductId, out var product))
                {
                    _logger.LogWarning("Product {ProductId} not found in catalog", item.ProductId);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                if (item.Quantity > product.StockQuantity)
                {
                    insufficientStockItems.Add((item.ProductId, product.Name, item.Quantity, product.StockQuantity));
                }
            }

            if (insufficientStockItems.Any())
            {
                var errorMessage = new ErrorResponse
                {
                    UserId = checkoutMessage.UserId,
                    Error = "Некоторых позиций нет в наличии",
                    Details = string.Join(", ", insufficientStockItems.Select(i => 
                        $"'{i.Name}': запрошено {i.Requested}, доступно {i.Available}"))
                };

                var errorBody = JsonSerializer.SerializeToUtf8Bytes(errorMessage);
                _channel.BasicPublish(
                    exchange: "",
                    routingKey: CartCheckoutErrorQueue,
                    basicProperties: null,
                    body: errorBody);

                _logger.LogWarning("Order creation failed due to insufficient stock for user {UserId}: {Error}", 
                    checkoutMessage.UserId, errorMessage.Details);
                _channel.BasicAck(ea.DeliveryTag, false);
                return;
            }

            // Если все товары в наличии, создаем заказ
            var createOrderDto = new CreateOrderDto
            {
                DeliveryAddress = checkoutMessage.DeliveryAddress,
                Items = checkoutMessage.Items.Select(i => new CreateOrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var order = orderService.CreateOrderAsync(createOrderDto, checkoutMessage.UserId).GetAwaiter().GetResult();
            _logger.LogInformation("Created order {OrderId} for user {UserId}", order.Id, checkoutMessage.UserId);
            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cart checkout message for user {UserId}", 
                JsonSerializer.Deserialize<CartCheckoutMessage>(message)?.UserId);
            _channel.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _channel?.Dispose();
        _connection?.Dispose();
    }

    private class CartCheckoutMessage
    {
        public Guid UserId { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public List<CartItemMessage> Items { get; set; } = new();
    }

    private class CartItemMessage
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    private class ErrorResponse
    {
        public Guid UserId { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
} 