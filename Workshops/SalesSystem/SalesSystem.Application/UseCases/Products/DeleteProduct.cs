using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Products;

public sealed class DeleteProduct
{
    private readonly IProductRepository _productRepository;

    public DeleteProduct(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result> ExecuteAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product is null)
        {
            return Result.Failure(
                new Error("Product.NotFound", "Product was not found."));
        }

        await _productRepository.DeleteAsync(productId);

        return Result.Success();
    }
}