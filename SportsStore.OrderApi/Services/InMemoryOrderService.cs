using SportsStore.OrderApi.Models;
using SportsStore.OrderApi.Services;
using SportsStore.Shared.Enums;

public class InMemoryOrderService : IOrderService
{
    private readonly List<Order> _orders = new();
    private readonly object _sync = new();

    // 👇 ADICIONA AQUI
    public InMemoryOrderService()
    {
        _orders.Add(new Order
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CustomerEmail = "test@test.com",
            TotalAmount = 100,
            Status = OrderStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow,
            Items = new List<OrderItem>()
        });
    }

    public void CreateOrder(Order order)
    {
        lock (_sync)
        {
            _orders.Add(order);
        }
    }

    public Order? GetById(Guid id)
    {
        lock (_sync)
        {
            return _orders.FirstOrDefault(o => o.Id == id);
        }
    }

    public IReadOnlyCollection<Order> GetAll()
    {
        lock (_sync)
        {
            return _orders
                .OrderByDescending(o => o.CreatedAtUtc)
                .ToList()
                .AsReadOnly();
        }
    }

    public void UpdateOrderStatus(Guid id, int status)
    {
        lock (_sync)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                order.Status = (OrderStatus)status;
            }
        }
    }
}