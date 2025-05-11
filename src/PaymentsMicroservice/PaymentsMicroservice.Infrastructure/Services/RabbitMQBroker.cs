using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentsMicroservice.Infrastructure.Services;

public class RabbitMQBroker : IMessageBroker, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQBroker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _exchangeName = "order.events";

    public RabbitMQBroker(
        ILogger<RabbitMQBroker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public Task PublishAsync<T>(T message)
    {
        try
        {
            var messageType = typeof(T).Name.ToLower();
            var routingKey = $"payments.{messageType}";
            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Published message of type {MessageType}", messageType);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message");
            throw;
        }
    }

    public Task SubscribeAsync<T>(string queueName, string routingKey, IMessageHandler<T> handler)
    {
        try
        {
            _logger.LogInformation("Setting up subscription to exchange {ExchangeName} with queue {QueueName} and routing key {RoutingKey}", 
                _exchangeName, queueName, routingKey);

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: queueName,
                exchange: _exchangeName,
                routingKey: routingKey);

            _logger.LogInformation("Queue {QueueName} bound to exchange {ExchangeName} with routing key {RoutingKey}", 
                queueName, _exchangeName, routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var scopedHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
                
                try
                {
                    _logger.LogInformation("Received message with routing key {RoutingKey}", ea.RoutingKey);
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received message content: {MessageContent}", messageJson);
                    
                    var message = JsonSerializer.Deserialize<T>(body);

                    if (message != null)
                    {
                        _logger.LogInformation("Processing message of type {MessageType}", typeof(T).Name);
                        await scopedHandler.HandleAsync(message);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogInformation("Message processed successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Received null message, skipping processing");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {ErrorMessage}", ex.Message);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Successfully subscribed to queue {QueueName} with routing key {RoutingKey}", queueName, routingKey);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to queue {QueueName}: {ErrorMessage}", queueName, ex.Message);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 