using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Products;

public sealed class GetAllProducts
{
    private readonly IProductRepository _productRepository;

    public GetAllProducts(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public Task<IReadOnlyList<Product>> ExecuteAsync()
    {
        return _productRepository.GetAllAsync();
    }
}