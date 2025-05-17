using System;

namespace NotificationMicroservice.Application.Services
{
    public class OrderInfo
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public required string Status { get; set; }
    }
} 