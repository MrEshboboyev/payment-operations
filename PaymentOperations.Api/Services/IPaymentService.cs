using PaymentOperations.Api.Models;
using Stripe;

namespace PaymentOperations.Api.Services;

// Payment Service Interface and Implementation
public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task<PaymentIntentResponse> ConfirmPaymentIntentAsync(ConfirmPaymentIntentRequest request);
    Task<RefundResponse> RefundPaymentAsync(CreateRefundRequest request);
    Task<CreatePaymentMethodResponse> CreatePaymentMethodAsync(CreatePaymentMethodRequest request);
}

public class PaymentService(
    PaymentIntentService paymentIntentService, 
    RefundService refundService,
    PaymentMethodService paymentMethodService) : IPaymentService
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
                PaymentMethod = request.PaymentMethodId // PaymentMethodId from frontend
            };

            // Confirm the payment intent using Stripe's API
            var paymentIntent = await paymentIntentService.ConfirmAsync(request.PaymentIntentId, options);

            // Return a response with payment intent details
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

    public async Task<CreatePaymentMethodResponse> CreatePaymentMethodAsync(CreatePaymentMethodRequest request)
    {
        try
        {
            // Use the token provided from the frontend instead of card details
            if (string.IsNullOrEmpty(request.Token))
            {
                throw new ApplicationException("Payment token is required.");
            }

            var options = new PaymentMethodCreateOptions
            {
                Type = "card",  // Assuming we're using "card" type. This can be extended to other types if needed.
                Card = new PaymentMethodCardOptions
                {
                    Token = request.Token  // Use the token from the frontend (not card details)
                }
            };

            // Create the payment method using Stripe's API
            var paymentMethod = await paymentMethodService.CreateAsync(options);

            // Return response with payment method details
            return new CreatePaymentMethodResponse
            {
                PaymentMethodId = paymentMethod.Id,
                CardBrand = paymentMethod.Card.Brand,
                CardLast4 = paymentMethod.Card.Last4,
                ExpiryMonth = paymentMethod.Card.ExpMonth,
                ExpiryYear = paymentMethod.Card.ExpYear
            };
        }
        catch (StripeException ex)
        {
            throw new ApplicationException($"Error creating payment method: {ex.Message}", ex);
        }
    }
}
