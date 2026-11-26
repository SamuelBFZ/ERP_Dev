namespace ERP.Api.Modules.Sales.Application;

public record CreateSaleRequest(Guid CustomerId, Guid ProductId, int Quantity);

public record SaleResult(Guid SaleId, decimal TotalAmount, int RemainingStock);
