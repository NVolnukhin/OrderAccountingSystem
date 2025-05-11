using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderMicroservice.Application.Interfaces;
using OrderMicroservice.Contracts.DTOs.Order;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderMicroservice.Infrastructure.Services;

public class RabbitMQMessageBroker : IMessageBroker, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBroker> _logger;

    private const string OrderCreatedExchange = "order.created";
    private const string OrderStatusChangedExchange = "order.status.changed";

    public RabbitMQMessageBroker(IConfiguration configuration, ILogger<RabbitMQMessageBroker> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchanges
        _channel.ExchangeDeclare(OrderCreatedExchange, ExchangeType.Fanout);
        _channel.ExchangeDeclare(OrderStatusChangedExchange, ExchangeType.Fanout);
    }

    public Task PublishOrderCreatedAsync(OrderResponseDto order)
    {
        try
        {
            var message = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: OrderCreatedExchange,
                routingKey: string.Empty,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Order created message published for order {OrderId}", order.Id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing order created message for order {OrderId}", order.Id);
            throw;
        }
    }

    public Task PublishOrderStatusChangedAsync(Guid orderId, string status)
    {
        try
        {
            var message = JsonSerializer.Serialize(new { OrderId = orderId, Status = status });
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: OrderStatusChangedExchange,
                routingKey: string.Empty,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Order status changed message published for order {OrderId}", orderId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing order status changed message for order {OrderId}", orderId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 