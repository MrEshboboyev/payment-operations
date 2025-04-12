using Microsoft.AspNetCore.Mvc;
using PaymentOperations.Api.Models;
using PaymentOperations.Api.Services;

namespace PaymentOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(
    IPaymentService paymentService, 
    ILogger<PaymentsController> logger) : ControllerBase
{
    [HttpPost("create-intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            // Validate request
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0");
            }

            var response = await paymentService.CreatePaymentIntentAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating payment intent");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPost("create-payment-method")]
    public async Task<ActionResult<CreatePaymentMethodResponse>> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("Payment token is required");
            }

            var response = await paymentService.CreatePaymentMethodAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating payment method");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }


    [HttpPost("confirm-intent")]
    public async Task<ActionResult<PaymentIntentResponse>> ConfirmPaymentIntent([FromBody] ConfirmPaymentIntentRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.PaymentIntentId))
            {
                return BadRequest("Payment intent ID is required");
            }

            if (string.IsNullOrEmpty(request.PaymentMethodId))
            {
                return BadRequest("Payment method ID is required");
            }

            // Call the service to confirm the payment intent
            var response = await paymentService.ConfirmPaymentIntentAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming payment intent");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPost("refund")]
    public async Task<ActionResult<RefundResponse>> RefundPayment([FromBody] CreateRefundRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.PaymentIntentId))
            {
                return BadRequest("Payment intent ID is required");
            }

            var response = await paymentService.RefundPaymentAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refunding payment");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
