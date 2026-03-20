namespace SportsStore.Shared.Contracts;

public class OrderSubmitted
{
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime SubmittedAtUtc { get; set; }

    public List<OrderSubmittedItem> Items { get; set; } = new();
}

public class OrderSubmittedItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}