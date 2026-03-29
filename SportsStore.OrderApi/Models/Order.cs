using SportsStore.Shared.Enums;

namespace SportsStore.OrderApi.Models;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}