using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderApi.Data;
using SportsStore.OrderApi.Models;

namespace SportsStore.OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly OrderDbContext _context;

    public ProductsController(OrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(products);
    }
}