using ERP.Api.Modules.Inventory.Domain;

namespace ERP.Api.Modules.Catalog.Domain;

public class Product
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public InventoryItem? Inventory { get; set; }
}
