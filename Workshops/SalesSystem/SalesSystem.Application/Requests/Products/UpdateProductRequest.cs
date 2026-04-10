namespace SalesSystem.Application.Requests.Products;

public sealed record UpdateProductRequest(
    Guid ProductId,
    int ProductNumber,
    string Name,
    decimal NetPrice,
    decimal VatRate);