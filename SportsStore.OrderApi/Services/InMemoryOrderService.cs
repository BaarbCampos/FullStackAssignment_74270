using SportsStore.OrderApi.Models;

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
}