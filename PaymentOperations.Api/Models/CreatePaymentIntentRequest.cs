namespace PaymentOperations.Api.Models;

public class CreatePaymentIntentRequest
{
    public long Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Description { get; set; }
    public string CustomerId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}
