namespace SportsStore.Shared.Contracts;

public class PaymentRejected
{
    public Guid OrderId { get; set; }
    public DateTime RejectedAtUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}