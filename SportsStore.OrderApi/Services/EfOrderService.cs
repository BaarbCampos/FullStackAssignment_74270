using Microsoft.EntityFrameworkCore;
using SportsStore.OrderApi.Data;
using SportsStore.OrderApi.Models;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderApi.Services;

public class EfOrderService : IOrderService
{
    private readonly OrderDbContext _context;

    public EfOrderService(OrderDbContext context)
    {
        _context = context;
    }

    public void CreateOrder(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    public Order? GetById(Guid id)
    {
        return _context.Orders
            .Include(o => o.Items)
            .FirstOrDefault(o => o.Id == id);
    }

    public IReadOnlyCollection<Order> GetAll()
    {
        return _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToList()
            .AsReadOnly();
    }

    public void UpdateOrderStatus(Guid id, int status)
    {
        var order = _context.Orders.FirstOrDefault(o => o.Id == id);

        if (order != null)
        {
            order.Status = (OrderStatus)status;
            _context.SaveChanges();
        }
    }
}