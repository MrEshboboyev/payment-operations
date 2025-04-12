namespace PaymentOperations.Api.Models;

// Payment Plan DTOs
public class CreatePaymentPlanRequest
{
    public long TotalAmount { get; set; }
    public string Currency { get; set; } = "usd";
    public int NumberOfInstallments { get; set; }
    public string CustomerId { get; set; }
    public string PaymentMethodId { get; set; }
    public string Description { get; set; }
}
