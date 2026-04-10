namespace SalesSystem.Application.Requests.Products;

public sealed record CreateProductRequest(
    int ProductNumber,
    string Name,
    decimal NetPrice,
    decimal VatRate,
    int StockQuantity,
    bool IsActive = true);