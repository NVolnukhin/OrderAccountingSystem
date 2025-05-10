using System.Text;
using System.Text.Json;
using CartMicroservice.Contracts.Messages;
using CartMicroservice.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CartMicroservice.Infrastructure.Services;

public class RabbitMQMessageBroker : IMessageBroker, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBroker> _logger;
    private const string CartCheckoutExchange = "cart.checkout";

    public RabbitMQMessageBroker(IConnection connection, ILogger<RabbitMQMessageBroker> logger)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        _logger = logger;

        // Declare exchange
        _channel.ExchangeDeclare(CartCheckoutExchange, ExchangeType.Fanout, durable: true);
    }

    public Task PublishCartCheckoutAsync(CartCheckoutMessage message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: CartCheckoutExchange,
                routingKey: string.Empty,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Cart checkout message published for user {UserId}", message.UserId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing cart checkout message for user {UserId}", message.UserId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
} 