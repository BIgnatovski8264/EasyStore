using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyStore.Data;
using EasyStore.Data.Entities;

namespace EasyStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplyController(AppDbContext context) : ControllerBase
{
    [HttpPost("add-stock")]
    public async Task<IActionResult> AddStock(int productId, double quantity)
    {
        Product? product = await context.Products.FindAsync(productId);

        if (product == null)
        {
            return NotFound("Продуктът не съществува.");
        }

        product.StockQuantity = product.StockQuantity + quantity;

        Sale record = new Sale
        {
            ProductId = product.Id,
            ProductName = "[ДОСТАВКА] " + product.Name,
            Quantity = quantity,
            TotalPrice = 0, 
            SaleDate = DateTime.UtcNow,
            GroupGuid = "SUPPLY-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper()
        };

        context.Sales.Add(record);
        await context.SaveChangesAsync();

        return Ok(record);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetSupplyHistory()
    {
        List<Sale> history = await context.Sales
            .Where(s => s.ProductName.StartsWith("[ДОСТАВКА]"))
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        return Ok(history);
    }
}
