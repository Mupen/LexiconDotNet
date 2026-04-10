using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Products;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Products;

public sealed class ChangeProductStatus
{
    private readonly IProductRepository _productRepository;

    public ChangeProductStatus(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result> ExecuteAsync(ChangeProductStatusRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);

        if (product is null)
        {
            return Result.Failure(
                new Error("Product.NotFound", "Product was not found."));
        }

        Result result = Result.Success();

        if (request.IsActive && !product.IsActive)
        {
            result = product.Activate();
        }
        else if (!request.IsActive && product.IsActive)
        {
            result = product.Deactivate();
        }

        if (result.IsFailure)
        {
            return result;
        }

        await _productRepository.UpdateAsync(product);

        return Result.Success();
    }
}