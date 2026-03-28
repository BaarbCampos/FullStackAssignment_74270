using SportsStore.OrderApi.Models;

namespace SportsStore.OrderApi.Services;

public interface IOrderService
{
    void CreateOrder(Order order);
    Order? GetById(Guid id);
    IReadOnlyCollection<Order> GetAll();
    void UpdateOrderStatus(Guid id, int status);
}