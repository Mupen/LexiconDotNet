using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Products;

public sealed class GetAvailableProducts
{
    private readonly IProductRepository _productRepository;

    public GetAvailableProducts(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<IReadOnlyList<Product>> ExecuteAsync()
    {
        var products = await _productRepository.GetAllAsync();

        var availableProducts = products
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .ToList();

        return availableProducts;
    }
}