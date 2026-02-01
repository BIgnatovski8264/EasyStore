using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyStore.Data;
using EasyStore.Data.Entities;

namespace EasyStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<Category> categories = await context.Categories.ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        Category? cat = await context.Categories.FindAsync(id);
        return cat == null ? NotFound() : Ok(cat);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category cat)
    {
        context.Categories.Add(cat);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Category updatedCat)
    {
        Category? cat = await context.Categories.FindAsync(id);
        if (cat == null)
            return NotFound();

        cat.Name = updatedCat.Name;
        await context.SaveChangesAsync();
        return Ok(cat);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool hasProducts = await context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
            return BadRequest("Категорията не е празна!");

        Category? cat = await context.Categories.FindAsync(id);
        if (cat == null)
            return NotFound();

        context.Categories.Remove(cat);
        await context.SaveChangesAsync();
        return Ok("Изтрита!");
    }
}
