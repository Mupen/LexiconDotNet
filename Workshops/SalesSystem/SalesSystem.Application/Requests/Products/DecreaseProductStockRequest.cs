namespace SalesSystem.Application.Requests.Products;

public sealed record DecreaseProductStockRequest(
    Guid ProductId,
    int Quantity);