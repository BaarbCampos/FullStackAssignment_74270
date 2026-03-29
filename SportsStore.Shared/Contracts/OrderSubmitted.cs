namespace SportsStore.Shared.Contracts;

public class OrderSubmitted
{
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime SubmittedAtUtc { get; set; }

    public List<OrderSubmittedItem> Items { get; set; } = new();
}