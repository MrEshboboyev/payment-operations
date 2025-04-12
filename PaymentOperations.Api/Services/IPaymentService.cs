using PaymentOperations.Api.Models;
using Stripe;

namespace PaymentOperations.Api.Services;

// Payment Service Interface and Implementation
public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task<PaymentIntentResponse> ConfirmPaymentIntentAsync(ConfirmPaymentIntentRequest request);
    Task<RefundResponse> RefundPaymentAsync(CreateRefundRequest request);
}

public class PaymentService(
    PaymentIntentService paymentIntentService, 
    RefundService refundService) : IPaymentService
{
    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                Metadata = request.Metadata,
                SetupFutureUsage = "off_session",
            };

            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                options.Customer = request.CustomerId;
            }

            var paymentIntent = await paymentIntentService.CreateAsync(options);

            return new PaymentIntentResponse
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency
            };
        }
        catch (StripeException ex)
        {
            throw new ApplicationException($"Error creating payment intent: {ex.Message}", ex);
        }
    }

    public async Task<PaymentIntentResponse> ConfirmPaymentIntentAsync(ConfirmPaymentIntentRequest request)
    {
        try
        {
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = request.PaymentMethodId
            };

            var paymentIntent = await paymentIntentService.ConfirmAsync(request.PaymentIntentId, options);

            return new PaymentIntentResponse
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency
            };
        }
        catch (StripeException ex)
        {
            throw new ApplicationException($"Error confirming payment intent: {ex.Message}", ex);
        }
    }

    public async Task<RefundResponse> RefundPaymentAsync(CreateRefundRequest request)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = request.PaymentIntentId,
                Amount = request.Amount,
                Reason = request.Reason == null ? null :
                         request.Reason.ToLower() switch
                         {
                             "duplicate" => "duplicate",
                             "fraudulent" => "fraudulent",
                             "requested_by_customer" => "requested_by_customer",
                             _ => null
                         }
            };

            var refund = await refundService.CreateAsync(options);

            return new RefundResponse
            {
                RefundId = refund.Id,
                Status = refund.Status,
                Amount = refund.Amount,
                Currency = refund.Currency
            };
        }
        catch (StripeException ex)
        {
            throw new ApplicationException($"Error refunding payment: {ex.Message}", ex);
        }
    }
}
