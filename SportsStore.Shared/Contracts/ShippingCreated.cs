namespace SportsStore.Shared.Contracts;

public class ShippingCreated
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}