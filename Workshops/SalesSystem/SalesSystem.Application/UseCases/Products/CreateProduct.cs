using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Products;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.UseCases.Products;

public sealed class CreateProduct
{
    private readonly IProductRepository _productRepository;

    public CreateProduct(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result<Product>> ExecuteAsync(CreateProductRequest request)
    {
        var existing = await _productRepository
            .GetProductByNumberAsync(request.ProductNumber);

        if (existing is not null)
        {
            return Result<Product>.Failure(
                new Error("Product.DuplicateProductNumber", $"Product number {request.ProductNumber} already exists."));
        }

        var result = Product.Create(
            request.ProductNumber,
            request.Name,
            request.NetPrice,
            request.VatRate,
            request.StockQuantity,
            request.IsActive);

        if (result.IsFailure)
        {
            return result;
        }

        await _productRepository.AddAsync(result.Value);

        return result;
    }
}