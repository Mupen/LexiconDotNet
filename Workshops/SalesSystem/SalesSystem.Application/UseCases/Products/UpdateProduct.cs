using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Products;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Products;

public sealed class UpdateProduct
{
    private readonly IProductRepository _productRepository;

    public UpdateProduct(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result> ExecuteAsync(UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);

        if (product is null)
        {
            return Result.Failure(
                new Error("Product.NotFound", "Product was not found."));
        }

        var existing = await _productRepository.GetProductByNumberAsync(request.ProductNumber);

        if (existing is not null && existing.Id != request.ProductId)
        {
            return Result.Failure(
                new Error("Product.DuplicateProductNumber", $"Product number {request.ProductNumber} already exists."));
        }

        var result = product.Update(
            request.ProductNumber,
            request.Name,
            request.NetPrice,
            request.VatRate);

        if (result.IsFailure)
        {
            return result;
        }

        await _productRepository.UpdateAsync(product);

        return Result.Success();
    }
}