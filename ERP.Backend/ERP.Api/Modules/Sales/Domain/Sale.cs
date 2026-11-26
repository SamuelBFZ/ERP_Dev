namespace ERP.Api.Modules.Sales.Domain;

public class Sale
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public List<SaleDetail> Details { get; set; } = [];
}
