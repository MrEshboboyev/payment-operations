namespace PaymentOperations.Api.Models;

public class WebhookResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string EventType { get; set; }
}