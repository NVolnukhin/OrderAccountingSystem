using DeliveryMicroservice.Application.Interfaces;
using DeliveryMicroservice.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveryController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Delivery>> GetDelivery(Guid id)
    {
        var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
        if (delivery == null)
            return NotFound();

        return Ok(delivery);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Delivery>>> GetUserDeliveries(Guid userId)
    {
        var deliveries = await _deliveryService.GetDeliveriesByUserIdAsync(userId);
        return Ok(deliveries);
    }

    [HttpPost("update-status")]
    public async Task<ActionResult<Delivery>> UpdateDeliveryStatus([FromBody] UpdateDeliveryStatusRequest request)
    {
        try
        {
            var delivery = await _deliveryService.UpdateDeliveryStatusAsync(request.DeliveryId, request.Status);
            return Ok(delivery);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

public class UpdateDeliveryStatusRequest
{
    public Guid DeliveryId { get; set; }
    public string Status { get; set; } = string.Empty;
} 