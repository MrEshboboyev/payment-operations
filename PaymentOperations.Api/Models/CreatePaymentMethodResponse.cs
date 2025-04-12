namespace PaymentOperations.Api.Models;

public class CreatePaymentMethodResponse
{
    public string PaymentMethodId { get; set; }
    public string CardBrand { get; set; }
    public string CardLast4 { get; set; }
    public long ExpiryMonth { get; set; }
    public long ExpiryYear { get; set; }
}
