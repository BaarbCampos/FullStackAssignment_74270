using AutoMapper;
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
    private readonly ILogger<OrdersController> _logger;
    private readonly IMapper _mapper;

    public OrdersController(
        IOrderService orderService,
        IMessagePublisher messagePublisher,
        OrderDbContext context,
        ILogger<OrdersController> logger,
        IMapper mapper)
    {
        _orderService = orderService;
        _messagePublisher = messagePublisher;
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout([FromBody] CheckoutRequestDto request)
    {
        _logger.LogInformation("Checkout started for customer {CustomerEmail}", request?.CustomerEmail);

        if (request == null || request.Items == null || !request.Items.Any())
        {
            _logger.LogWarning("Checkout failed because the request had no items.");
            return BadRequest("The order must contain at least one item.");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            _logger.LogWarning("Checkout failed because customer email was missing.");
            return BadRequest("Customer email is required.");
        }

        var requestedProductIds = request.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        _logger.LogInformation("Looking up {Count} requested products.", requestedProductIds.Count);

        var products = await _context.Products
            .Where(p => requestedProductIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != requestedProductIds.Count)
        {
            _logger.LogWarning("Checkout failed because one or more products do not exist.");
            return BadRequest("One or more selected products do not exist.");
        }

        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                _logger.LogWarning("Checkout failed because product {ProductId} had invalid quantity {Quantity}.",
                    item.ProductId, item.Quantity);
                return BadRequest("Quantity must be greater than zero.");
            }

            var product = products.First(p => p.Id == item.ProductId);

            var mappedItem = _mapper.Map<OrderItem>(item);
            mappedItem.ProductId = product.Id;
            mappedItem.ProductName = product.Name;
            mappedItem.UnitPrice = product.Price;

            orderItems.Add(mappedItem);
        }

        var totalAmount = orderItems.Sum(i => i.Quantity * i.UnitPrice);

        var order = _mapper.Map<Order>(request);
        order.Id = Guid.NewGuid();
        order.TotalAmount = totalAmount;
        order.Status = OrderStatus.Submitted;
        order.CreatedAtUtc = DateTime.UtcNow;
        order.Items = orderItems;

        _logger.LogInformation("Creating order {OrderId} for customer {CustomerEmail} with total {TotalAmount}.",
            order.Id, order.CustomerEmail, order.TotalAmount);

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

        _logger.LogInformation("Publishing OrderSubmitted event for order {OrderId}.", order.Id);

        await _messagePublisher.PublishAsync("order-submitted", orderSubmitted);

        _orderService.UpdateOrderStatus(order.Id, (int)OrderStatus.InventoryPending);

        _logger.LogInformation("Order {OrderId} status updated to {Status}.",
            order.Id, OrderStatus.InventoryPending);

        var response = _mapper.Map<CheckoutResponseDto>(order);
        response.OrderId = order.Id;
        response.Status = OrderStatus.InventoryPending;
        response.TotalAmount = order.TotalAmount;
        response.Message = "Order submitted successfully and is waiting for inventory confirmation.";

        _logger.LogInformation("Checkout completed successfully for order {OrderId}.", order.Id);

        return Ok(response);
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<Order>> GetAll()
    {
        _logger.LogInformation("Fetching all orders.");
        var orders = _orderService.GetAll();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Order> GetById(Guid id)
    {
        _logger.LogInformation("Fetching order by ID {OrderId}.", id);

        var order = _orderService.GetById(id);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} was not found.", id);
            return NotFound();
        }

        return Ok(order);
    }

    [HttpGet("{id:guid}/status")]
    public ActionResult<object> GetStatus(Guid id)
    {
        _logger.LogInformation("Fetching status for order {OrderId}.", id);

        var order = _orderService.GetById(id);

        if (order == null)
        {
            _logger.LogWarning("Status check failed because order {OrderId} was not found.", id);
            return NotFound();
        }

        return Ok(new
        {
            order.Id,
            Status = order.Status.ToString()
        });
    }
}