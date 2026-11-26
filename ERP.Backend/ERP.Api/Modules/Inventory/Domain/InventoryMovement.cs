namespace ERP.Api.Modules.Inventory.Domain;

public class InventoryMovement
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string MovementType { get; set; } = "";
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
