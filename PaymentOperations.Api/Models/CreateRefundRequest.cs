namespace PaymentOperations.Api.Models;

// Refund DTOs
public class CreateRefundRequest
{
    public string PaymentIntentId { get; set; }
    public long? Amount { get; set; }
    public string Reason { get; set; }
}
