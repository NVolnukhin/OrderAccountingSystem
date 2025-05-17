using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationMicroservice.Application.Services;
using Shared.Contracts.Events;

namespace NotificationMicroservice.Application.EventHandlers
{
    public class DeliveryEventHandler
    {
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly ILogger<DeliveryEventHandler> _logger;

        public DeliveryEventHandler(
            INotificationService notificationService,
            IOrderService orderService,
            ILogger<DeliveryEventHandler> logger)
        {
            _notificationService = notificationService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task HandleDeliveryStartedEvent(DeliveryStartedEvent @event)
        {
            _logger.LogInformation("Handling DeliveryStartedEvent for order {OrderId}, delivery {DeliveryId}", @event.OrderId, @event.DeliveryId);
            
            try
            {
                _logger.LogInformation("Creating notification for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                var notification = await _notificationService.CreateNotificationAsync(
                    @event.UserId,
                    @event.OrderId,
                    "Доставка началась",
                    $"Доставка заказа #{@event.OrderId} началась. Номер отслеживания: {@event.TrackingNumber}",
                    "DeliveryStarted"
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
                _logger.LogError(ex, "Error handling DeliveryStartedEvent for order {OrderId}, delivery {DeliveryId}", @event.OrderId, @event.DeliveryId);
                throw;
            }
        }

        public async Task HandleDeliveryStatusUpdatedEvent(DeliveryStatusUpdatedEvent @event)
        {
            _logger.LogInformation("Handling DeliveryStatusUpdatedEvent for order {OrderId}, delivery {DeliveryId}", @event.OrderId, @event.DeliveryId);
            
            try
            {
                _logger.LogInformation("Getting order info for order {OrderId}", @event.OrderId);
                var orderInfo = await _orderService.GetOrderInfoAsync(@event.OrderId);
                
                if (orderInfo == null)
                {
                    _logger.LogWarning("Order info not found for order {OrderId}", @event.OrderId);
                    return;
                }

                _logger.LogInformation("Creating notification for order {OrderId}, user {UserId}", @event.OrderId, orderInfo.UserId);
                var notification = await _notificationService.CreateNotificationAsync(
                    orderInfo.UserId,
                    @event.OrderId,
                    "Статус доставки обновлен",
                    $"Статус доставки заказа #{@event.OrderId} изменен на: {@event.Status}",
                    "DeliveryStatusUpdated"
                );

                if (notification == null)
                {
                    _logger.LogWarning("Failed to create notification for order {OrderId}, user {UserId}", @event.OrderId, orderInfo.UserId);
                }
                else
                {
                    _logger.LogInformation("Successfully created notification for order {OrderId}, user {UserId}", @event.OrderId, orderInfo.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling DeliveryStatusUpdatedEvent for order {OrderId}, delivery {DeliveryId}", @event.OrderId, @event.DeliveryId);
                throw;
            }
        }

        public async Task HandleDeliveryCompletedEvent(DeliveryCompletedEvent @event)
        {
            _logger.LogInformation("Handling DeliveryCompletedEvent for order {OrderId}, delivery {DeliveryId}", @event.OrderId, @event.DeliveryId);
            
            try
            {
                _logger.LogInformation("Creating notification for order {OrderId}, user {UserId}", @event.OrderId, @event.UserId);
                var notification = await _notificationService.CreateNotificationAsync(
                    @event.UserId,
                    @event.OrderId,
                    "Доставка завершена",
                    $"Доставка заказа #{@event.OrderId} успешно завершена",
                    "DeliveryCompleted"
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
                _logger.LogError(ex, "Error handling DeliveryCompletedEvent for order {OrderId}, delivery {DeliveryId}", @event.OrderId, @event.DeliveryId);
                throw;
            }
        }
    }
} 