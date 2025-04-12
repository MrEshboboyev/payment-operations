namespace PaymentOperations.Api.Models;

public class RefundResponse
{
    public string RefundId { get; set; }
    public string Status { get; set; }
    public long Amount { get; set; }
    public string Currency { get; set; }
}
