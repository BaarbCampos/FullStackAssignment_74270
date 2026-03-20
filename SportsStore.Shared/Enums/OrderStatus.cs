namespace SportsStore.Shared.Enums;

public enum OrderStatus
{
    Cart,
    Submitted,
    InventoryPending,
    InventoryConfirmed,
    InventoryFailed,
    PaymentPending,
    PaymentApproved,
    PaymentFailed,
    ShippingPending,
    ShippingCreated,
    Completed,
    Failed
}