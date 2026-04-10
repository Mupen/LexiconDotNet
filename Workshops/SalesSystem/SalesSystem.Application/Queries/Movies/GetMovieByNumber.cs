using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Movies;

public sealed class GetMovieByNumber
{
    private readonly IMovieRepository _movieRepository;

    public GetMovieByNumber(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<Result<Movie>> ExecuteAsync(int movieNumber)
    {
        var movie = await _movieRepository.GetMovieByNumberAsync(movieNumber);

        if (movie is null)
        {
            return Result<Movie>.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        return Result<Movie>.Success(movie);
    }
}