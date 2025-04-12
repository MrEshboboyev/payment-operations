namespace PaymentOperations.Api.Models;

// Webhook DTOs
public class WebhookRequest
{
    public string Data { get; set; }
    public string Signature { get; set; }
}
