using SportsStore.OrderApi.Models;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderApi.Services;

public class InMemoryOrderService : IOrderService
{
    private readonly List<Order> _orders = new();

    public void CreateOrder(Order order)
    {
        _orders.Add(order);
    }

    public Order? GetById(Guid id)
    {
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    // 🔥 MÉTODO CORRETO
    public void UpdateOrderStatus(Guid id, int status)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);

        if (order != null)
        {
            order.Status = (OrderStatus)status;
        }
    }
}