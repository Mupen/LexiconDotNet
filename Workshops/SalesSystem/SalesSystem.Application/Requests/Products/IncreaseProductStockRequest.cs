namespace SalesSystem.Application.Requests.Products;

public sealed record IncreaseProductStockRequest(
    Guid ProductId,
    int Quantity);