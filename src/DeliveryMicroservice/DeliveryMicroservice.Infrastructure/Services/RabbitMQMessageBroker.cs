using System.Text;
using System.Text.Json;
using DeliveryMicroservice.Domain.Interfaces;
using DeliveryMicroservice.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;
using Shared.Contracts.Events;

namespace DeliveryMicroservice.Infrastructure.Services;

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

    public RabbitMQMessageBroker(
        IConfiguration configuration,
        ILogger<RabbitMQMessageBroker> logger,
        IServiceProvider serviceProvider)
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

        // Объявляем обменники
        _channel.ExchangeDeclare(OrderEventsExchange, ExchangeType.Topic, true);
        _channel.ExchangeDeclare(PaymentEventsExchange, ExchangeType.Topic, true);
        _channel.ExchangeDeclare(DeliveryEventsExchange, ExchangeType.Topic, true);
    }

    public Task PublishAsync<T>(T message) where T : class
    {
        try
        {
            string exchange;
            string routingKey;
            
            if (message is DeliveryStatusUpdatedEvent deliveryEvent)
            {
                exchange = DeliveryEventsExchange;
                routingKey = $"delivery.status.{deliveryEvent.Status.ToString().ToLower()}";
            }
            else
            {
                exchange = DeliveryEventsExchange;
                routingKey = $"delivery.{typeof(T).Name.ToLower()}";
            }

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Published message of type {MessageType} to exchange {Exchange} with routing key: {RoutingKey}", 
                typeof(T).Name, exchange, routingKey);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message");
            throw;
        }
    }

    public async Task SubscribeToOrderEventsAsync()
    {
        var queueName = "delivery.order.events";
        _channel.QueueDeclare(queueName, true, false, false);
        _channel.QueueBind(queueName, OrderEventsExchange, "order.created");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received message: {Message} with routing key: {RoutingKey}", message, routingKey);

                // Обработка события создания заказа
                if (routingKey == "order.created")
                {
                    var orderCreated = JsonSerializer.Deserialize<Shared.Contracts.Events.OrderCreatedEvent>(message);
                    if (orderCreated != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();
                        
                        // Создаем запись о доставке
                        var delivery = new Domain.Entities.Delivery
                        {
                            OrderId = orderCreated.OrderId,
                            UserId = orderCreated.UserId,
                            Address = orderCreated.DeliveryAddress,
                            Status = Domain.Entities.DeliveryStatus.Pending
                        };

                        await deliveryService.CreateDeliveryAsync(delivery);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        };

        _channel.BasicConsume(queue: queueName,
                            autoAck: true,
                            consumer: consumer);

        await Task.CompletedTask;
    }

    public async Task SubscribeToPaymentEventsAsync()
    {
        var queueName = "delivery.payment.events";
        _logger.LogInformation("Setting up payment events subscription with queue: {QueueName}", queueName);
        
        _channel.QueueDeclare(queueName, true, false, false);
        _channel.QueueBind(queueName, PaymentEventsExchange, "payment.completed");
        _logger.LogInformation("Queue {QueueName} bound to exchange {Exchange} with routing key payment.completed", 
            queueName, PaymentEventsExchange);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received payment event message: {Message} with routing key: {RoutingKey}", message, routingKey);

                // Обработка события обработки платежа
                if (routingKey == "payment.completed")
                {
                    _logger.LogInformation("Processing payment.completed event");
                    var paymentProcessed = JsonSerializer.Deserialize<PaymentCompletedEvent>(message);
                    if (paymentProcessed != null)
                    {
                        _logger.LogInformation("Payment completed for OrderId: {OrderId}", paymentProcessed.OrderId);
                        using var scope = _serviceProvider.CreateScope();
                        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();
                        
                        // Обновляем статус доставки на "Preparing"
                        var delivery = await deliveryService.GetDeliveryByOrderIdAsync(paymentProcessed.OrderId);
                        if (delivery != null)
                        {
                            _logger.LogInformation("Found delivery for OrderId: {OrderId}, current status: {Status}", 
                                paymentProcessed.OrderId, delivery.Status);
                            await deliveryService.UpdateDeliveryStatusAsync(delivery.DeliveryId, Domain.Entities.DeliveryStatus.Preparing);
                            _logger.LogInformation("Updated delivery status to Preparing for OrderId: {OrderId}", paymentProcessed.OrderId);
                        }
                        else
                        {
                            _logger.LogWarning("No delivery found for OrderId: {OrderId}", paymentProcessed.OrderId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize PaymentCompletedEvent from message: {Message}", message);
                    }
                }
                else
                {
                    _logger.LogWarning("Received unexpected routing key: {RoutingKey}", routingKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment event message");
            }
        };

        _channel.BasicConsume(queue: queueName,
                            autoAck: true,
                            consumer: consumer);

        _logger.LogInformation("Successfully subscribed to payment events");
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        _disposed = true;
    }
} 