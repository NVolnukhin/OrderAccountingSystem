using System;
using System.Threading.Tasks;
using NotificationMicroservice.Application.Models;

namespace NotificationMicroservice.Application.Services
{
    public interface IOrderService
    {
        Task<OrderInfo> GetOrderInfoAsync(Guid orderId);
    }
} 