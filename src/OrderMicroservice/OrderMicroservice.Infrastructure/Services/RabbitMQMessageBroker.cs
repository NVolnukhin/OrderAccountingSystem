using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderMicroservice.Application.Interfaces;
using OrderMicroservice.Contracts.DTOs.Order;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;
using Shared.Contracts.Events;

namespace OrderMicroservice.Infrastructure.Services;

public class RabbitMQMessageBroker : IMessageBroker, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBroker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;

    private const string OrderEventsExchange = "order.events";
    private const string PaymentEventsExchange = "payment.events";
    private const string DeliveryEventsExchange = "delivery.events";

    public RabbitMQMessageBroker(IConfiguration configuration, ILogger<RabbitMQMessageBroker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchanges
        _channel.ExchangeDeclare(
            exchange: OrderEventsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _channel.ExchangeDeclare(
            exchange: PaymentEventsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _channel.ExchangeDeclare(
            exchange: DeliveryEventsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public Task PublishOrderCreatedAsync(OrderResponseDto order)
    {
        try
        {
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                DeliveryAddress = order.DeliveryAddress,
                TotalPrice = order.TotalPrice,
                Amount = order.TotalPrice // если Amount нужен отдельно
            };
            var message = JsonSerializer.Serialize(orderCreatedEvent);
            _logger.LogInformation("Publishing order created message: {MessageContent}", message);
            _logger.LogInformation("OrderId being published: {OrderId}", order.Id);
            
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: OrderEventsExchange,
                routingKey: "order.created",
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
                exchange: OrderEventsExchange,
                routingKey: "order.status.changed",
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

    public async Task SubscribeToPaymentEventsAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMQMessageBroker));
        }

        // Очереди для событий оплаты
        const string paymentCompletedQueue = "payment.completed";
        const string paymentFailedQueue = "payment.failed";
        const string paymentRefundedQueue = "payment.refunded";

        // Подписка на очередь успешной оплаты
        var paymentCompletedConsumer = new EventingBasicConsumer(_channel);
        paymentCompletedConsumer.Received += async (model, ea) =>
        {
            if (_disposed)
            {
                _logger.LogWarning("Message broker is disposed, skipping message processing");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<PaymentCompletedEvent>>();
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received payment completed message: {Message} with routing key: {RoutingKey}", message, ea.RoutingKey);
                var paymentEvent = JsonSerializer.Deserialize<PaymentCompletedEvent>(message);
                if (paymentEvent != null)
                {
                    await handler.HandleAsync(paymentEvent);
                    if (!_disposed)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize payment completed event");
                    if (!_disposed)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment completed event");
                if (!_disposed)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            }
        };
        _channel.QueueDeclare(paymentCompletedQueue, true, false, false);
        _channel.QueueBind(paymentCompletedQueue, PaymentEventsExchange, "payment.completed");
        _channel.BasicConsume(paymentCompletedQueue, false, paymentCompletedConsumer);

        // Подписка на очередь неуспешной оплаты
        var paymentFailedConsumer = new EventingBasicConsumer(_channel);
        paymentFailedConsumer.Received += async (model, ea) =>
        {
            if (_disposed)
            {
                _logger.LogWarning("Message broker is disposed, skipping message processing");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<PaymentFailedEvent>>();
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received payment failed message: {Message}", message);
                var paymentEvent = JsonSerializer.Deserialize<PaymentFailedEvent>(message);
                if (paymentEvent != null)
                {
                    await handler.HandleAsync(paymentEvent);
                    if (!_disposed)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize payment failed event");
                    if (!_disposed)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment failed event");
                if (!_disposed)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            }
        };
        _channel.QueueDeclare(paymentFailedQueue, true, false, false);
        _channel.QueueBind(paymentFailedQueue, PaymentEventsExchange, "payment.failed");
        _channel.BasicConsume(paymentFailedQueue, false, paymentFailedConsumer);

        // Подписка на очередь возврата средств
        var paymentRefundedConsumer = new EventingBasicConsumer(_channel);
        paymentRefundedConsumer.Received += async (model, ea) =>
        {
            if (_disposed)
            {
                _logger.LogWarning("Message broker is disposed, skipping message processing");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<PaymentRefundedEvent>>();
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received payment refunded message: {Message}", message);
                var paymentEvent = JsonSerializer.Deserialize<PaymentRefundedEvent>(message);
                if (paymentEvent != null)
                {
                    await handler.HandleAsync(paymentEvent);
                    if (!_disposed)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize payment refunded event");
                    if (!_disposed)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment refunded event");
                if (!_disposed)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            }
        };
        _channel.QueueDeclare(paymentRefundedQueue, true, false, false);
        _channel.QueueBind(paymentRefundedQueue, PaymentEventsExchange, "payment.refunded");
        _channel.BasicConsume(paymentRefundedQueue, false, paymentRefundedConsumer);
    }

    public async Task SubscribeToDeliveryEventsAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMQMessageBroker));
        }

        const string deliveryStatusQueue = "order.delivery.status";

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            if (_disposed)
            {
                _logger.LogWarning("Message broker is disposed, skipping message processing");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<DeliveryStatusUpdatedEvent>>();
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received delivery status message: {Message} with routing key: {RoutingKey}", message, ea.RoutingKey);
                var deliveryEvent = JsonSerializer.Deserialize<DeliveryStatusUpdatedEvent>(message);
                if (deliveryEvent != null)
                {
                    await handler.HandleAsync(deliveryEvent);
                    if (!_disposed)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize delivery status event");
                    if (!_disposed)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing delivery status event");
                if (!_disposed)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            }
        };

        _channel.QueueDeclare(deliveryStatusQueue, true, false, false);
        _channel.QueueBind(deliveryStatusQueue, DeliveryEventsExchange, "delivery.status.*");
        _channel.BasicConsume(deliveryStatusQueue, false, consumer);

        _logger.LogInformation("Successfully subscribed to delivery status events");
        await Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(IMessageHandler<T> handler) where T : class
    {
        var queueName = typeof(T).Name.ToLower();
        _channel.QueueDeclare(queueName, true, false, false);
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = System.Text.Encoding.UTF8.GetString(body);
            var eventObj = JsonSerializer.Deserialize<T>(message);
            if (eventObj != null)
                await handler.HandleAsync(eventObj);
            _channel.BasicAck(ea.DeliveryTag, false);
        };
        _channel.BasicConsume(queueName, false, consumer);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 