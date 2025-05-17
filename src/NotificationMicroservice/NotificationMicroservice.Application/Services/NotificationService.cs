using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationMicroservice.Domain.Entities;
using NotificationMicroservice.Domain.Repositories;

namespace NotificationMicroservice.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IServiceScopeFactory scopeFactory, 
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(Guid userId, Guid orderId, string title, string message, string notificationType)
        {
            _logger.LogInformation("Creating notification for user {UserId}, order {OrderId}, type {Type}", 
                userId, orderId, notificationType);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var notification = new Notification(userId, orderId, title, message, notificationType);
                await repository.AddAsync(notification);
                _logger.LogInformation("Successfully created notification {NotificationId}", notification.Id);
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}, order {OrderId}", userId, orderId);
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            return await repository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Notification>> GetUnreadUserNotificationsAsync(Guid userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            return await repository.GetUnreadByUserIdAsync(userId);
        }

        public async Task MarkNotificationAsReadAsync(Guid notificationId)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var notification = await repository.GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.MarkAsRead();
                await repository.UpdateAsync(notification);
            }
        }

        public async Task DeleteNotificationAsync(Guid notificationId)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            await repository.DeleteAsync(notificationId);
        }
    }
} 