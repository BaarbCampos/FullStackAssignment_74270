using SportsStore.OrderApi.Models;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderApi.Services;

public class InMemoryOrderService : IOrderService
{
    private readonly List<Order> _orders = new();

    public Order CreateOrder(Order order)
    {
        _orders.Add(order);
        return order;
    }

    public Order? GetById(Guid id)
    {
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public bool UpdateStatus(Guid id, OrderStatus status)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);

        if (order is null)
        {
            return false;
        }

        order.Status = status;
        return true;
    }
}