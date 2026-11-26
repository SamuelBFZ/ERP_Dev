using ERP.Api.Data;
using ERP.Api.Modules.Inventory.Domain;
using ERP.Api.Modules.Sales.Application;
using ERP.Api.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Modules.Sales.Presentation;

[ApiController]
[Route("api/sales")]
public class SalesController(ErpDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleRequest request)
    {
        if (request.Quantity < 1)
        {
            return BadRequest(new { error = "Quantity must be at least 1." });
        }

        var username = configuration["Auth:Username"];
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
        if (user is null)
        {
            return BadRequest(new { error = "Seeded user was not found." });
        }

        var customer = await db.Customers.FindAsync(request.CustomerId);
        if (customer is null)
        {
            return BadRequest(new { error = "Customer was not found." });
        }

        var product = await db.Products.FindAsync(request.ProductId);
        if (product is null || !product.IsActive)
        {
            return BadRequest(new { error = "Product was not found." });
        }

        var stock = await db.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == product.Id);
        if (stock is null || stock.Quantity < request.Quantity)
        {
            return BadRequest(new { error = "Not enough stock." });
        }

        var now = DateTime.UtcNow;
        var subtotal = product.Price * request.Quantity;
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            UserId = user.Id,
            SaleDate = now,
            TotalAmount = subtotal,
            Status = "completed"
        };
        sale.Details.Add(new SaleDetail
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPrice = product.Price,
            Subtotal = subtotal
        });

        stock.Quantity -= request.Quantity;
        stock.UpdatedAt = now;

        db.Sales.Add(sale);
        db.InventoryMovements.Add(new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            MovementType = "out",
            Quantity = request.Quantity,
            CreatedAt = now
        });

        await db.SaveChangesAsync();
        return Ok(new SaleResult(sale.Id, sale.TotalAmount, stock.Quantity));
    }
}
