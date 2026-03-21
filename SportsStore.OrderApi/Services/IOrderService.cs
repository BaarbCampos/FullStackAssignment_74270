using SportsStore.OrderApi.Models;

namespace SportsStore.OrderApi.Services;

public interface IOrderService
{
    void CreateOrder(Order order);
    Order? GetById(Guid id);

    // 🔥 TEM QUE EXISTIR ISSO
    void UpdateOrderStatus(Guid id, int status);
}