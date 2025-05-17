using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotificationMicroservice.Domain.Entities;

namespace NotificationMicroservice.Application.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(Guid userId, Guid orderId, string title, string message, string notificationType);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadUserNotificationsAsync(Guid userId);
        Task MarkNotificationAsReadAsync(Guid notificationId);
        Task DeleteNotificationAsync(Guid notificationId);
    }
} 