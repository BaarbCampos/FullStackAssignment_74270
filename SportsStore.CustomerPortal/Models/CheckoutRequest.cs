namespace SportsStore.CustomerPortal.Models;

public class CheckoutRequest
{
    public string CustomerEmail { get; set; } = string.Empty;
    public List<CheckoutItem> Items { get; set; } = new();
}