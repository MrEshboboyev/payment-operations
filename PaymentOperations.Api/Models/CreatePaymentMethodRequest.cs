namespace PaymentOperations.Api.Models;

// Payment Method DTOs
public class CreatePaymentMethodRequest
{
    public string Type { get; set; } = "card";
    public CardInfo Card { get; set; }
    public BillingDetails BillingDetails { get; set; }
}
