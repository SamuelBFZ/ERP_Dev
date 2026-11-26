using ERP.Api.Data;
using ERP.Api.Modules.Customers.Application;
using ERP.Api.Modules.Customers.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Modules.Customers.Presentation;

[ApiController]
[Route("api/customers")]
public class CustomersController(ErpDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var customers = await db.Customers
            .OrderBy(x => x.Name)
            .ToListAsync();
        return Ok(customers);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required." });
        }

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        if (email is not null && await db.Customers.AnyAsync(x => x.Email == email))
        {
            return BadRequest(new { error = "Email is already registered." });
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return Ok(customer);
    }
}
