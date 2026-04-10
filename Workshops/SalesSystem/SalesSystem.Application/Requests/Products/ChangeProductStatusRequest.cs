namespace SalesSystem.Application.Requests.Products;

public sealed record ChangeProductStatusRequest(
    Guid ProductId,
    bool IsActive);