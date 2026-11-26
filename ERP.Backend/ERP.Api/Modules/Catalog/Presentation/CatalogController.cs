using ERP.Api.Data;
using ERP.Api.Modules.Catalog.Application;
using ERP.Api.Modules.Catalog.Domain;
using ERP.Api.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Modules.Catalog.Presentation;

[ApiController]
[Route("api/catalog")]
public class CatalogController(ErpDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await db.Products
            .OrderBy(x => x.Name)
            .Select(x => new CatalogItem(
                x.Id,
                x.Name,
                x.Description,
                x.Price,
                x.IsActive,
                x.CategoryId,
                x.Category!.Name,
                x.Inventory != null ? x.Inventory.Quantity : 0))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> ListCategories()
    {
        var categories = await db.Categories
            .OrderBy(x => x.Name)
            .ToListAsync();
        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required." });
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return Ok(category);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required." });
        }

        if (request.Price < 0)
        {
            return BadRequest(new { error = "Price cannot be negative." });
        }

        if (request.InitialQuantity < 0)
        {
            return BadRequest(new { error = "Initial quantity cannot be negative." });
        }

        var category = await db.Categories.FindAsync(request.CategoryId);
        if (category is null)
        {
            return BadRequest(new { error = "Category was not found." });
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Price = request.Price,
            IsActive = true
        };

        db.Products.Add(product);
        db.InventoryItems.Add(new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = request.InitialQuantity,
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
        return Ok(new CatalogItem(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.IsActive,
            product.CategoryId,
            category.Name,
            request.InitialQuantity));
    }
}
