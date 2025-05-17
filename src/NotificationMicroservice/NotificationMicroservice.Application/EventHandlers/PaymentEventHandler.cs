using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationMicroservice.Application.Services;
using Shared.Contracts.Events;

namespace NotificationMicroservice.Application.EventHandlers
{
    public class PaymentEventHandler
    {
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly ILogger<PaymentEventHandler> _logger;

        public PaymentEventHandler(
            INotificationService notificationService,
            IOrderService orderService,
            ILogger<PaymentEventHandler> logger)
        {
            _notificationService = notificationService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task HandlePaymentCompletedEvent(PaymentCompletedEvent @event)
        {
            _logger.LogInformation("Handling PaymentCompletedEvent for order {OrderId}, payment {PaymentId}", @event.OrderId, @event.PaymentId);
            
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
                var paymentNotification = await _notificationService.CreateNotificationAsync(
                    orderInfo.UserId,
                    @event.OrderId,
                    "Оплата завершена",
                    $"Оплата заказа #{@event.OrderId} на сумму {@event.Amount} успешно завершена",
                    "PaymentCompleted"
                );

                if (paymentNotification == null)
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
                _logger.LogError(ex, "Error handling PaymentCompletedEvent for order {OrderId}, payment {PaymentId}", @event.OrderId, @event.PaymentId);
                throw;
            }
        }

        public async Task HandlePaymentFailedEvent(PaymentFailedEvent @event)
        {
            _logger.LogInformation("Handling PaymentFailedEvent for order {OrderId}, payment {PaymentId}", @event.OrderId, @event.PaymentId);
            
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
                var paymentNotification = await _notificationService.CreateNotificationAsync(
                    orderInfo.UserId,
                    @event.OrderId,
                    "Ошибка оплаты",
                    $"Ошибка при оплате заказа #{@event.OrderId} на сумму {@event.Amount}: {@event.ErrorMessage}",
                    "PaymentFailed"
                );

                if (paymentNotification == null)
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
                _logger.LogError(ex, "Error handling PaymentFailedEvent for order {OrderId}, payment {PaymentId}", @event.OrderId, @event.PaymentId);
                throw;
            }
        }

        public async Task HandlePaymentRefundedEvent(PaymentRefundedEvent @event)
        {
            _logger.LogInformation("Handling PaymentRefundedEvent for order {OrderId}, payment {PaymentId}", @event.OrderId, @event.PaymentId);
            
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
                var paymentNotification = await _notificationService.CreateNotificationAsync(
                    orderInfo.UserId,
                    @event.OrderId,
                    "Возврат средств",
                    $"Средства по заказу #{@event.OrderId} на сумму {@event.Amount} успешно возвращены",
                    "PaymentRefunded"
                );

                if (paymentNotification == null)
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
                _logger.LogError(ex, "Error handling PaymentRefundedEvent for order {OrderId}, payment {PaymentId}", @event.OrderId, @event.PaymentId);
                throw;
            }
        }
    }
} 