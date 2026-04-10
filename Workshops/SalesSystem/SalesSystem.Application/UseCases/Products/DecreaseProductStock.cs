using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Products;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Products;

public sealed class DecreaseProductStock
{
    private readonly IProductRepository _productRepository;

    public DecreaseProductStock(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result> ExecuteAsync(DecreaseProductStockRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);

        if (product is null)
        {
            return Result.Failure(
                new Error("Product.NotFound", "Product was not found."));
        }

        if (!product.IsActive)
        {
            return Result.Failure(
                new Error("Product.Inactive", "Cannot sell inactive product."));
        }

        var result = product.DecreaseStock(request.Quantity);

        if (result.IsFailure)
        {
            return result;
        }

        await _productRepository.UpdateAsync(product);

        return Result.Success();
    }
}