using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Products;

public sealed class GetProductByNumber
{
    private readonly IProductRepository _productRepository;

    public GetProductByNumber(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result<Product>> ExecuteAsync(int productNumber)
    {
        var product = await _productRepository.GetProductByNumberAsync(productNumber);

        if (product is null)
        {
            return Result<Product>.Failure(
                new Error("Product.NotFound", "Product was not found."));
        }

        return Result<Product>.Success(product);
    }
}