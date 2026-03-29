namespace SportsStore.Shared.DTOs;

public class CheckoutRequestDto
{
    public string CustomerEmail { get; set; } = string.Empty;
    public List<CheckoutRequestItemDto> Items { get; set; } = new();
}