using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Infrastructure.Repositories;

public sealed class InMemoryMovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = [];

    public Task<IReadOnlyList<Movie>> GetAllAsync()
    {
        IReadOnlyList<Movie> result = _movies.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<Movie?> GetByIdAsync(Guid id)
    {
        Movie? movie = _movies.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(movie);
    }

    public Task<Movie?> GetMovieByNumberAsync(int movieNumber)
    {
        Movie? movie = _movies.FirstOrDefault(x => x.MovieNumber == movieNumber);
        return Task.FromResult(movie);
    }
    public Task AddAsync(Movie movie)
    {
        ArgumentNullException.ThrowIfNull(movie);

        _movies.Add(movie);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Movie movie)
    {
        ArgumentNullException.ThrowIfNull(movie);

        int index = _movies.FindIndex(existing => existing.Id == movie.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Movie with id '{movie.Id}' was not found.");
        }

        _movies[index] = movie;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        Movie? movie = _movies.FirstOrDefault(x => x.Id == id);

        if (movie is null)
        {
            throw new InvalidOperationException(
                $"Movie with id '{id}' was not found.");
        }

        _movies.Remove(movie);
        return Task.CompletedTask;
    }
}