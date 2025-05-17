using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationMicroservice.Application.Services;
using Shared.Contracts.Events;

namespace NotificationMicroservice.Application.EventHandlers
{
    public class OrderEventHandler
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrderEventHandler> _logger;

        public OrderEventHandler(
            INotificationService notificationService,
            ILogger<OrderEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleOrderCreatedEvent(OrderCreatedEvent @event)
        {
            _logger.LogInformation("Handling OrderCreatedEvent for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
            
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(
                    @event.UserId,
                    @event.OrderId,
                    "Заказ создан",
                    $"Ваш заказ #{@event.OrderId} успешно создан.",
                    "OrderCreated"
                );

                if (notification == null)
                {
                    _logger.LogWarning("Failed to create notification for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                }
                else
                {
                    _logger.LogInformation("Successfully created notification for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OrderCreatedEvent for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                throw;
            }
        }

        public async Task HandleOrderStatusChangedEvent(OrderStatusChangedEvent @event)
        {
            _logger.LogInformation("Handling OrderStatusChangedEvent for order {OrderId}, user {UserId}, status {Status}", 
                @event.OrderId, @event.UserId, @event.Status);
            
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(
                    @event.UserId,
                    @event.OrderId,
                    "Статус заказа обновлен",
                    $"Статус вашего заказа #{@event.OrderId} изменен на: {@event.Status}",
                    "OrderStatusChanged"
                );

                if (notification == null)
                {
                    _logger.LogWarning("Failed to create notification for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                }
                else
                {
                    _logger.LogInformation("Successfully created notification for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OrderStatusChangedEvent for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                throw;
            }
        }
    }
} 