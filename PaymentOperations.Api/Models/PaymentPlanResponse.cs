namespace PaymentOperations.Api.Models;

public class PaymentPlanResponse
{
    public string PlanId { get; set; }
    public int NumberOfInstallments { get; set; }
    public long InstallmentAmount { get; set; }
    public List<PaymentIntentResponse> Installments { get; set; }
}
