using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Infrastructure.Repositories;

public sealed class InMemoryShowingRepository : IShowingRepository
{
    private readonly List<Showing> _showings = [];

    public Task<IReadOnlyList<Showing>> GetAllAsync()
    {
        IReadOnlyList<Showing> result = _showings.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<Showing?> GetByIdAsync(Guid id)
    {
        Showing? showing = _showings.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(showing);
    }

    public Task<IReadOnlyList<Showing>> GetByMovieIdAsync(Guid movieId)
    {
        IReadOnlyList<Showing> result = _showings
            .Where(x => x.MovieId == movieId)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToList();

        return Task.FromResult(result);
    }

    public Task AddAsync(Showing showing)
    {
        ArgumentNullException.ThrowIfNull(showing);

        _showings.Add(showing);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Showing showing)
    {
        ArgumentNullException.ThrowIfNull(showing);

        int index = _showings.FindIndex(existing => existing.Id == showing.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Showing with id '{showing.Id}' was not found.");
        }

        _showings[index] = showing;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        Showing? showing = _showings.FirstOrDefault(x => x.Id == id);

        if (showing is null)
        {
            throw new InvalidOperationException(
                $"Showing with id '{id}' was not found.");
        }

        _showings.Remove(showing);
        return Task.CompletedTask;
    }
}