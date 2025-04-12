using Microsoft.AspNetCore.Mvc;
using PaymentOperations.Api.Models;
using PaymentOperations.Api.Services;

namespace PaymentOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentPlansController(
    IPaymentPlanService paymentPlanService, 
    ILogger<PaymentPlansController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<PaymentPlanResponse>> CreatePaymentPlan([FromBody] CreatePaymentPlanRequest request)
    {
        try
        {
            // Validate request
            if (request.TotalAmount <= 0)
            {
                return BadRequest("Total amount must be greater than 0");
            }

            if (request.NumberOfInstallments <= 0)
            {
                return BadRequest("Number of installments must be greater than 0");
            }

            if (string.IsNullOrEmpty(request.CustomerId))
            {
                return BadRequest("Customer ID is required");
            }

            if (string.IsNullOrEmpty(request.PaymentMethodId))
            {
                return BadRequest("Payment method ID is required");
            }

            var response = await paymentPlanService.CreatePaymentPlanAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating payment plan");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
