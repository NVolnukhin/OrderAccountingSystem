using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationMicroservice.Application.EventHandlers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;

namespace NotificationMicroservice.Infrastructure.Services
{
    public class RabbitMQService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const string OrderEventsExchange = "order.events";
        private const string PaymentEventsExchange = "payment.events";
        private const string DeliveryEventsExchange = "delivery.events";

        public RabbitMQService(
            IConfiguration configuration,
            ILogger<RabbitMQService> logger,
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

            // Создаем и привязываем очереди
            SetupOrderQueue();
            SetupPaymentQueue();
            SetupDeliveryQueue();
        }

        private void SetupOrderQueue()
        {
            var queueName = "notification.order.events";
            _channel.QueueDeclare(queueName, true, false, false);
            
            // Привязываем очередь к обменнику с нужными routing keys
            _channel.QueueBind(queueName, OrderEventsExchange, "order.created");
            _channel.QueueBind(queueName, OrderEventsExchange, "order.status.*");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("Получено сообщение: {Message} с routing key: {RoutingKey}", message, routingKey);

                    using var scope = _serviceProvider.CreateScope();
                    var orderEventHandler = scope.ServiceProvider.GetRequiredService<OrderEventHandler>();

                    switch (routingKey)
                    {
                        case "order.created":
                            var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                            if (orderCreated != null)
                            {
                                await orderEventHandler.HandleOrderCreatedEvent(orderCreated);
                            }
                            break;
                        case var key when key.StartsWith("order.status."):
                            var orderStatusChanged = JsonSerializer.Deserialize<OrderStatusChangedEvent>(message);
                            if (orderStatusChanged != null)
                            {
                                await orderEventHandler.HandleOrderStatusChangedEvent(orderStatusChanged);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения");
                }
            };

            _channel.BasicConsume(queue: queueName,
                                autoAck: true,
                                consumer: consumer);
        }

        private void SetupPaymentQueue()
        {
            var queueName = "notification.payment.events";
            _channel.QueueDeclare(queueName, true, false, false);
            
            // Привязываем очередь к обменнику с нужными routing keys
            _channel.QueueBind(queueName, PaymentEventsExchange, "payment.completed");
            _channel.QueueBind(queueName, PaymentEventsExchange, "payment.failed");
            _channel.QueueBind(queueName, PaymentEventsExchange, "payment.refunded");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("Получено сообщение: {Message} с routing key: {RoutingKey}", message, routingKey);

                    using var scope = _serviceProvider.CreateScope();
                    var paymentEventHandler = scope.ServiceProvider.GetRequiredService<PaymentEventHandler>();

                    switch (routingKey)
                    {
                        case "payment.completed":
                            var paymentCompleted = JsonSerializer.Deserialize<PaymentCompletedEvent>(message);
                            if (paymentCompleted != null)
                            {
                                await paymentEventHandler.HandlePaymentCompletedEvent(paymentCompleted);
                            }
                            break;
                        case "payment.failed":
                            var paymentFailed = JsonSerializer.Deserialize<PaymentFailedEvent>(message);
                            if (paymentFailed != null)
                            {
                                await paymentEventHandler.HandlePaymentFailedEvent(paymentFailed);
                            }
                            break;
                        case "payment.refunded":
                            var paymentRefunded = JsonSerializer.Deserialize<PaymentRefundedEvent>(message);
                            if (paymentRefunded != null)
                            {
                                await paymentEventHandler.HandlePaymentRefundedEvent(paymentRefunded);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения");
                }
            };

            _channel.BasicConsume(queue: queueName,
                                autoAck: true,
                                consumer: consumer);
        }

        private void SetupDeliveryQueue()
        {
            var queueName = "notification.delivery.events";
            _channel.QueueDeclare(queueName, true, false, false);
            
            // Привязываем очередь к обменнику с нужными routing keys
            _channel.QueueBind(queueName, DeliveryEventsExchange, "delivery.started");
            _channel.QueueBind(queueName, DeliveryEventsExchange, "delivery.completed");
            _channel.QueueBind(queueName, DeliveryEventsExchange, "delivery.status.*");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("Получено сообщение: {Message} с routing key: {RoutingKey}", message, routingKey);

                    using var scope = _serviceProvider.CreateScope();
                    var deliveryEventHandler = scope.ServiceProvider.GetRequiredService<DeliveryEventHandler>();

                    switch (routingKey)
                    {
                        case "delivery.started":
                            var deliveryStarted = JsonSerializer.Deserialize<DeliveryStartedEvent>(message);
                            if (deliveryStarted != null)
                            {
                                await deliveryEventHandler.HandleDeliveryStartedEvent(deliveryStarted);
                            }
                            break;
                        case "delivery.completed":
                            var deliveryCompleted = JsonSerializer.Deserialize<DeliveryCompletedEvent>(message);
                            if (deliveryCompleted != null)
                            {
                                await deliveryEventHandler.HandleDeliveryCompletedEvent(deliveryCompleted);
                            }
                            break;
                        case var key when key.StartsWith("delivery.status."):
                            var deliveryStatusUpdated = JsonSerializer.Deserialize<DeliveryStatusUpdatedEvent>(message);
                            if (deliveryStatusUpdated != null)
                            {
                                await deliveryEventHandler.HandleDeliveryStatusUpdatedEvent(deliveryStatusUpdated);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения");
                }
            };

            _channel.BasicConsume(queue: queueName,
                                autoAck: true,
                                consumer: consumer);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис RabbitMQ запущен");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
} 