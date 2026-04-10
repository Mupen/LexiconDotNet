using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Movies;

public sealed class GetActiveMovies
{
    private readonly IMovieRepository _movieRepository;

    public GetActiveMovies(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<IReadOnlyList<Movie>> ExecuteAsync()
    {
        var movies = await _movieRepository.GetAllAsync();

        var activeMovies = movies
            .Where(m => m.IsActive)
            .ToList();

        return activeMovies;
    }
}