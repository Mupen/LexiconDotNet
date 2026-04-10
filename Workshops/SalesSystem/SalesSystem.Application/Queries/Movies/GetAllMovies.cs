using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Movies;

public sealed class GetAllMovies
{
    private readonly IMovieRepository _movieRepository;

    public GetAllMovies(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public Task<IReadOnlyList<Movie>> ExecuteAsync()
    {
        return _movieRepository.GetAllAsync();
    }
}