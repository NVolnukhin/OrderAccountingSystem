using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DeliveryMicroservice.Application.Interfaces;
using DeliveryMicroservice.Domain.Entities;
using DeliveryMicroservice.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace DeliveryMicroservice.Application.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<DeliveryService> _logger;

    public DeliveryService(
        IDeliveryRepository deliveryRepository,
        IMessageBroker messageBroker,
        ILogger<DeliveryService> logger)
    {
        _deliveryRepository = deliveryRepository;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task<Delivery> CreateDeliveryAsync(Delivery delivery)
    {
        delivery.CreatedAt = DateTime.UtcNow;
        var created = await _deliveryRepository.CreateAsync(delivery);
        return created;
    }

    public async Task<Delivery?> GetDeliveryByIdAsync(Guid id)
    {
        return await _deliveryRepository.GetByIdAsync(id);
    }

    public async Task<Delivery?> GetDeliveryByOrderIdAsync(Guid orderId)
    {
        return await _deliveryRepository.GetByOrderIdAsync(orderId);
    }

    public async Task<Delivery> UpdateDeliveryStatusAsync(Guid id, string status)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(id);
        if (delivery == null)
        {
            throw new KeyNotFoundException($"Delivery with ID {id} not found");
        }

        // Проверяем, что статус допустим
        var validStatuses = new[]
        {
            DeliveryStatus.Pending,
            DeliveryStatus.Preparing,
            DeliveryStatus.Shipped,
            DeliveryStatus.Delivered,
            DeliveryStatus.Canceled
        };
        if (!Array.Exists(validStatuses, s => s == status))
        {
            throw new ArgumentException($"Invalid delivery status: {status}");
        }

        delivery.Status = status;
        delivery.UpdatedAt = DateTime.UtcNow;

        // Если статус меняется на Shipped, генерируем TrackingNumber
        if (status == DeliveryStatus.Shipped)
        {
            delivery.TrackingNumber = GenerateTrackingNumber();
        }

        var updated = await _deliveryRepository.UpdateAsync(delivery);

        // Публикуем событие изменения статуса
        await _messageBroker.PublishAsync(new DeliveryStatusUpdatedEvent
        {
            DeliveryId = delivery.DeliveryId,
            OrderId = delivery.OrderId,
            Status = status
        });

        _logger.LogInformation("Delivery {DeliveryId} status updated to {Status}", id, status);
        return updated;
    }

    public async Task<IEnumerable<Delivery>> GetDeliveriesByUserIdAsync(Guid userId)
    {
        return await _deliveryRepository.GetByUserIdAsync(userId);
    }

    public async Task<bool> DeleteDeliveryAsync(Guid id)
    {
        return await _deliveryRepository.DeleteAsync(id);
    }

    private string GenerateTrackingNumber()
    {
        // Генерация уникального номера для отслеживания
        return $"DEL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
} 