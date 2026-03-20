namespace SportsStore.Shared.Contracts;

public class InventoryConfirmed
{
    public Guid OrderId { get; set; }
    public DateTime ConfirmedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}