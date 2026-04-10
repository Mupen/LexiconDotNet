using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Movies;

public sealed class GetMovieById
{
    private readonly IMovieRepository _movieRepository;

    public GetMovieById(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<Result<Movie>> ExecuteAsync(Guid movieId)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);

        if (movie is null)
        {
            return Result<Movie>.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        return Result<Movie>.Success(movie);
    }
}