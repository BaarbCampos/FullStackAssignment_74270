using SportsStore.CustomerPortal.Models;

namespace SportsStore.CustomerPortal.Services;

public class CartService
{
    private readonly List<CartItem> _items = new();

    public List<CartItem> GetItems() => _items;

    public void AddToCart(CartItem item)
    {
        var existing = _items.FirstOrDefault(x => x.ProductId == item.ProductId);

        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            _items.Add(item);
        }
    }

    public void Remove(int productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
            _items.Remove(item);
    }

    public void Clear() => _items.Clear();

    public decimal GetTotal() => _items.Sum(x => x.Price * x.Quantity);
}