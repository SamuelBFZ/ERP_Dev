namespace ERP.Api.Modules.Customers.Application;

public record CreateCustomerRequest(string Name, string? Email, string? Phone, string? Address);
