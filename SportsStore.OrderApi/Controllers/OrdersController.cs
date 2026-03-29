using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderApi.Data;
using SportsStore.OrderApi.Messaging;
using SportsStore.OrderApi.Models;
using SportsStore.OrderApi.Services;
using SportsStore.Shared.Contracts;
using SportsStore.Shared.DTOs;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly OrderDbContext _context;

    public OrdersController(
        IOrderService orderService,
        IMessagePublisher messagePublisher,
        OrderDbContext context)
    {
        _orderService = orderService;
        _messagePublisher = messagePublisher;
        _context = context;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout([FromBody] CheckoutRequestDto request)
    {
        if (request == null || request.Items == null || !request.Items.Any())
        {
            return BadRequest("The order must contain at least one item.");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            return BadRequest("Customer email is required.");
        }

        var requestedProductIds = request.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .Where(p => requestedProductIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != requestedProductIds.Count)
        {
            return BadRequest("One or more selected products do not exist.");
        }

        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            }

            var product = products.First(p => p.Id == item.ProductId);

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        var totalAmount = orderItems.Sum(i => i.Quantity * i.UnitPrice);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerEmail = request.CustomerEmail,
            TotalAmount = totalAmount,
            Status = OrderStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow,
            Items = orderItems
        };

        _orderService.CreateOrder(order);

        var orderSubmitted = new OrderSubmitted
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            SubmittedAtUtc = DateTime.UtcNow,
            Items = order.Items.Select(i => new OrderSubmittedItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _messagePublisher.PublishAsync("order-submitted", orderSubmitted);

        _orderService.UpdateOrderStatus(order.Id, (int)OrderStatus.InventoryPending);

        var response = new CheckoutResponseDto
        {
            OrderId = order.Id,
            Status = OrderStatus.InventoryPending,
            TotalAmount = order.TotalAmount,
            Message = "Order submitted successfully and is waiting for inventory confirmation."
        };

        return Ok(response);
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<Order>> GetAll()
    {
        var orders = _orderService.GetAll();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Order> GetById(Guid id)
    {
        var order = _orderService.GetById(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpGet("{id:guid}/status")]
    public ActionResult<object> GetStatus(Guid id)
    {
        var order = _orderService.GetById(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            order.Id,
            Status = order.Status.ToString()
        });
    }
}