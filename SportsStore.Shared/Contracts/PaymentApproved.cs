namespace SportsStore.Shared.Contracts;

public class PaymentApproved
{
    public Guid OrderId { get; set; }
    public DateTime ApprovedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}