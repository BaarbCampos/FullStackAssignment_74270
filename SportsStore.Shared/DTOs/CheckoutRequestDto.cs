namespace SportsStore.Shared.DTOs;

public class CheckoutRequestDto
{
    public string CustomerEmail { get; set; } = string.Empty;
    public List<CheckoutItemDto> Items { get; set; } = new();
}

public class CheckoutItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}