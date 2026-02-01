using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyStore.Data;
using EasyStore.Data.Entities;

namespace EasyStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<Product> products = await context.Products.Include(p => p.Category).ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        Product? product = await context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        Product? product = await context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("add-multiple-stock")]
    public async Task<IActionResult> AddMultipleStock([FromBody] List<SupplyRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Липсват данни за доставка");

        string supplyCode = "IN-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

        foreach (SupplyRequest req in requests)
        {
            Product? product = await context.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId);
            if (product != null)
            {
                product.StockQuantity = product.StockQuantity + req.Quantity;

                Sale deliveryNote = new Sale
                {
                    ProductId = product.Id,
                    ProductName = "[ДОСТАВКА] " + product.Name,
                    Quantity = req.Quantity,
                    TotalPrice = 0,
                    SaleDate = DateTime.UtcNow,
                    GroupGuid = supplyCode
                };
                context.Sales.Add(deliveryNote);
            }
        }

        await context.SaveChangesAsync();
        return Ok(new { Message = "Доставката е отразена", Code = supplyCode });
    }
}

public class SupplyRequest
{
    public int ProductId { get; set; }
    public double Quantity { get; set; }
}
