using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Application.Interfaces;
using PaymentsMicroservice.Domain.Entities;

namespace PaymentsMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<Payment>> GetPaymentByOrderId(Guid orderId)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment for order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpPost("{paymentId}/refund")]
    public async Task<ActionResult<Payment>> RefundPayment(Guid paymentId)
    {
        try
        {
            var payment = await _paymentService.RefundPaymentAsync(paymentId);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpPut("{paymentId}/status")]
    public async Task<ActionResult<Payment>> UpdatePaymentStatus(Guid paymentId, [FromBody] PaymentStatus newStatus)
    {
        try
        {
            var payment = await _paymentService.UpdatePaymentStatusAsync(paymentId, newStatus);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status for payment {PaymentId}", paymentId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
} 