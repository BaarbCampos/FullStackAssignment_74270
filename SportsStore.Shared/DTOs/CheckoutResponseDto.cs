using SportsStore.Shared.Enums;

namespace SportsStore.Shared.DTOs;

public class CheckoutResponseDto
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}