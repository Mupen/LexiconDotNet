using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Movies;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.UseCases.Movies;

public sealed class CreateMovie
{
    private readonly IMovieRepository _movieRepository;

    public CreateMovie(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<Result<Movie>> ExecuteAsync(CreateMovieRequest request)
    {
        var existing = await _movieRepository
            .GetMovieByNumberAsync(request.MovieNumber);

        if (existing is not null)
        {
            return Result<Movie>.Failure(
                new Error("Movie.DuplicateMovieNumber", $"Movie number {request.MovieNumber} already exists."));
        }

        var result = Movie.Create(
            request.MovieNumber,
            request.Title,
            request.Description,
            request.YearReleased,
            request.AgeRating,
            request.Duration,
            request.IsActive);

        if (result.IsFailure)
        {
            return result;
        }

        await _movieRepository.AddAsync(result.Value);

        return result;
    }
}