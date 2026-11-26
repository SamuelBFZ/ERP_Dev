namespace ERP.Api.Modules.Catalog.Application;

public record CreateCategoryRequest(string Name, string? Description);

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    int InitialQuantity);

public record CatalogItem(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    Guid CategoryId,
    string CategoryName,
    int StockQuantity);
