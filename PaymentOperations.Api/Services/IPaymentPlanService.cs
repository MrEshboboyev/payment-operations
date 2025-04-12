using PaymentOperations.Api.Models;
using Stripe;

namespace PaymentOperations.Api.Services;

// Payment Plan Service Interface and Implementation
public interface IPaymentPlanService
{
    Task<PaymentPlanResponse> CreatePaymentPlanAsync(CreatePaymentPlanRequest request);
}

public class PaymentPlanService(
    PaymentIntentService paymentIntentService, 
    ILogger<PaymentPlanService> logger) : IPaymentPlanService
{
    public async Task<PaymentPlanResponse> CreatePaymentPlanAsync(CreatePaymentPlanRequest request)
    {
        try
        {
            // Calculate installment amount (divide by number of installments)
            long installmentAmount = request.TotalAmount / request.NumberOfInstallments;

            // Generate plan ID
            string planId = $"plan_{Guid.NewGuid().ToString("N")}";

            // Create installment payment intents
            var installments = new List<PaymentIntentResponse>();

            for (int i = 0; i < request.NumberOfInstallments; i++)
            {
                // For the last installment, add any remaining amount due to integer division
                long amount = installmentAmount;
                if (i == request.NumberOfInstallments - 1)
                {
                    amount += request.TotalAmount - (installmentAmount * request.NumberOfInstallments);
                }

                // Create metadata to track the installment
                var metadata = new Dictionary<string, string>
                    {
                        { "plan_id", planId },
                        { "installment_number", (i + 1).ToString() },
                        { "total_installments", request.NumberOfInstallments.ToString() }
                    };

                // First installment is captured immediately, rest are set for future capture
                bool isFirstInstallment = (i == 0);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = request.Currency,
                    Customer = request.CustomerId,
                    PaymentMethod = isFirstInstallment ? request.PaymentMethodId : null,
                    Description = $"{request.Description} - Installment {i + 1} of {request.NumberOfInstallments}",
                    Metadata = metadata,
                    CaptureMethod = isFirstInstallment ? "automatic" : "manual",
                    SetupFutureUsage = "off_session"
                };

                var paymentIntent = await paymentIntentService.CreateAsync(options);

                // If this is the first installment, confirm it immediately
                if (isFirstInstallment && paymentIntent.Status != "succeeded")
                {
                    var confirmOptions = new PaymentIntentConfirmOptions
                    {
                        PaymentMethod = request.PaymentMethodId
                    };

                    paymentIntent = await paymentIntentService.ConfirmAsync(paymentIntent.Id, confirmOptions);
                }

                installments.Add(new PaymentIntentResponse
                {
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id,
                    Status = paymentIntent.Status,
                    Amount = paymentIntent.Amount,
                    Currency = paymentIntent.Currency
                });
            }

            return new PaymentPlanResponse
            {
                PlanId = planId,
                NumberOfInstallments = request.NumberOfInstallments,
                InstallmentAmount = installmentAmount,
                Installments = installments
            };
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Error creating payment plan");
            throw new ApplicationException($"Error creating payment plan: {ex.Message}", ex);
        }
    }
}
