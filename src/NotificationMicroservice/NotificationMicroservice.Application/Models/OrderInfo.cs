using System;

namespace NotificationMicroservice.Application.Models
{
    public class OrderInfo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
} 