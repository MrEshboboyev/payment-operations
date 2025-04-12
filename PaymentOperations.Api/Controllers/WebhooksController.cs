using Microsoft.AspNetCore.Mvc;
using PaymentOperations.Api.Services;

namespace PaymentOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController(
    IWebhookService webhookService,
    IConfiguration configuration,
    ILogger<WebhooksController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            // Get the request body
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Get the signature from the request headers
            var signature = Request.Headers["Stripe-Signature"];

            // Get the webhook secret from configuration
            var endpointSecret = configuration["Stripe:WebhookSecret"];

            // Process the webhook
            var response = await webhookService.ProcessWebhookAsync(json, signature, endpointSecret);

            if (response.Success)
            {
                logger.LogInformation($"Webhook processed successfully. Event: {response.EventType}");
                return Ok(response);
            }
            else
            {
                logger.LogWarning($"Webhook processing failed: {response.Message}");
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling webhook");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
