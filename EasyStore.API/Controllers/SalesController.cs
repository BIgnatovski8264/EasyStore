using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyStore.Data;
using EasyStore.Data.Entities;

namespace EasyStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSales()
    {
        List<Sale> sales = await _context.Sales
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        return Ok(sales);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProductsForStore()
    {
        var products = await _context.Products
            .OrderBy(p => p.Name)
            .Select(p => new {
                p.Id,
                p.Name,
                p.Price,
                p.StockQuantity,
                p.CategoryId
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost("sell-multiple")]
    public async Task<IActionResult> SellMultiple([FromBody] List<SaleRequestDto> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Количката е празна.");

        string cashierFromRequest = requests.First().CashierName;

        if (string.IsNullOrWhiteSpace(cashierFromRequest))
        {
            cashierFromRequest = "Неизвестен касиер";
        }

        string receiptCode = requests.First().GroupGuid;
        if (string.IsNullOrEmpty(receiptCode))
            receiptCode = "SALE-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        foreach (SaleRequestDto req in requests)
        {
            Product? product = await _context.Products.FindAsync(req.ProductId);
            if (product != null)
            {
                product.StockQuantity -= req.Quantity;

                Sale newSale = new Sale
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = req.Quantity,
                    TotalPrice = (decimal)req.Quantity * product.Price,
                    SaleDate = DateTime.UtcNow,
                    GroupGuid = receiptCode,
                    CashierName = cashierFromRequest
                };

                _context.Sales.Add(newSale);
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { ReceiptCode = receiptCode });
    }
}

public class SaleRequestDto
{
    public int ProductId { get; set; }
    public double Quantity { get; set; }
    public string GroupGuid { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
}
