using PaymentOperations.Api.Models;
using Stripe;

namespace PaymentOperations.Api.Services;

// Webhook Service Interface and Implementation
public interface IWebhookService
{
    Task<WebhookResponse> ProcessWebhookAsync(string requestBody, string signature, string endpointSecret);
}


public class WebhookService(ILogger<WebhookService> logger) : IWebhookService
{
    public async Task<WebhookResponse> ProcessWebhookAsync(
        string requestBody, 
        string signature, 
        string endpointSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                requestBody,
                signature,
                endpointSecret
            );

            // Handle the event based on its type
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentSucceeded(paymentIntent);
                    break;
                case "payment_intent.payment_failed":
                    var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentFailed(failedPaymentIntent);
                    break;
                case "charge.refunded":
                    var charge = stripeEvent.Data.Object as Charge;
                    await HandleChargeRefunded(charge);
                    break;
                    // Add more event handlers as needed
            }

            return new WebhookResponse
            {
                Success = true,
                Message = "Webhook processed successfully",
                EventType = stripeEvent.Type
            };
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Error processing webhook");
            return new WebhookResponse
            {
                Success = false,
                Message = $"Error processing webhook: {ex.Message}",
                EventType = null
            };
        }
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
    {
        logger.LogInformation($"Payment intent succeeded: {paymentIntent.Id}");

        // Check if this is part of a payment plan
        if (paymentIntent.Metadata.TryGetValue("plan_id", out string planId))
        {
            int installmentNumber = int.Parse(paymentIntent.Metadata["installment_number"]);
            int totalInstallments = int.Parse(paymentIntent.Metadata["total_installments"]);

            logger.LogInformation($"Installment {installmentNumber} of {totalInstallments} for plan {planId} succeeded");

            // Here you would typically update your database to mark this installment as paid
            // and possibly trigger the next installment if needed
        }

        // Implement your business logic here (e.g., update order status, send email confirmation, etc.)
        await Task.CompletedTask; // Placeholder for async implementation
    }

    private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
    {
        logger.LogWarning($"Payment intent failed: {paymentIntent.Id}");

        // Check if this is part of a payment plan
        if (paymentIntent.Metadata.TryGetValue("plan_id", out string planId))
        {
            int installmentNumber = int.Parse(paymentIntent.Metadata["installment_number"]);
            int totalInstallments = int.Parse(paymentIntent.Metadata["total_installments"]);

            logger.LogWarning($"Installment {installmentNumber} of {totalInstallments} for plan {planId} failed");

            // Here you would typically update your database and possibly notify the customer
        }

        // Implement failure handling logic (e.g., retry logic, notify customer, etc.)
        await Task.CompletedTask; // Placeholder for async implementation
    }

    private async Task HandleChargeRefunded(Charge charge)
    {
        logger.LogInformation($"Charge refunded: {charge.Id}");

        // Implement refund handling logic
        await Task.CompletedTask; // Placeholder for async implementation
    }
}