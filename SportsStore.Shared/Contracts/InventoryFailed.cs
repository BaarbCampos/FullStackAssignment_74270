namespace SportsStore.Shared.Contracts;

public class InventoryFailed
{
    public Guid OrderId { get; set; }
    public DateTime FailedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}