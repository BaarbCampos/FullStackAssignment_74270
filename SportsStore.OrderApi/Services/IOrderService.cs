using SportsStore.OrderApi.Models;

namespace SportsStore.OrderApi.Services;

public interface IOrderService
{
    Order CreateOrder(Order order);
    Order? GetById(Guid id);
}