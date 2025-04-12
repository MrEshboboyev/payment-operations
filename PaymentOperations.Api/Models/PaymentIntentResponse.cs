namespace PaymentOperations.Api.Models;

public class PaymentIntentResponse
{
    public string ClientSecret { get; set; }
    public string PaymentIntentId { get; set; }
    public string Status { get; set; }
    public long Amount { get; set; }
    public string Currency { get; set; }
}
