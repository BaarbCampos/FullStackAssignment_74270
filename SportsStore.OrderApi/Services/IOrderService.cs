using SportsStore.OrderApi.Models;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderApi.Services;

public interface IOrderService
{
    Order CreateOrder(Order order);
    Order? GetById(Guid id);
    bool UpdateStatus(Guid id, OrderStatus status);
}