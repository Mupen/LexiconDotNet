using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Infrastructure.Repositories;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = [];

    public Task<IReadOnlyList<Product>> GetAllAsync()
    {
        IReadOnlyList<Product> result = _products.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        Product? product = _products.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(product);
    }

    public Task<Product?> GetProductByNumberAsync(int productNumber)
    {
        Product? product = _products.FirstOrDefault(x => x.ProductNumber == productNumber);
        return Task.FromResult(product);
    }

    public Task AddAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        int index = _products.FindIndex(existing => existing.Id == product.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Product with id '{product.Id}' was not found.");
        }

        _products[index] = product;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        Product? product = _products.FirstOrDefault(x => x.Id == id);

        if (product is null)
        {
            throw new InvalidOperationException(
                $"Product with id '{id}' was not found.");
        }

        _products.Remove(product);
        return Task.CompletedTask;
    }
}