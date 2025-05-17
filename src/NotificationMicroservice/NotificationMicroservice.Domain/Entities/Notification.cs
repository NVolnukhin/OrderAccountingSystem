using System;

namespace NotificationMicroservice.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid OrderId { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsRead { get; private set; }
        public string NotificationType { get; private set; }

        private Notification() { } // Для EF Core

        public Notification(Guid userId, Guid orderId, string title, string message, string notificationType)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            OrderId = orderId;
            Title = title;
            Message = message;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
            NotificationType = notificationType;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
} 