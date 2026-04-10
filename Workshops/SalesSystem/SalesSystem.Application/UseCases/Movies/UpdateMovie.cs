using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Movies;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Movies;

public sealed class UpdateMovie
{
    private readonly IMovieRepository _movieRepository;

    public UpdateMovie(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<Result> ExecuteAsync(UpdateMovieRequest request)
    {
        var movie = await _movieRepository.GetByIdAsync(request.MovieId);

        if (movie is null)
        {
            return Result.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        var existing = await _movieRepository.GetMovieByNumberAsync(request.MovieNumber);

        if (existing is not null && existing.Id != request.MovieId)
        {
            return Result.Failure(
                new Error("Movie.DuplicateMovieNumber", $"Movie number {request.MovieNumber} already exists."));
        }

        var result = movie.Update(
            request.MovieNumber,
            request.Title,
            request.Description,
            request.YearReleased,
            request.AgeRating,
            request.Duration);

        if (result.IsFailure)
        {
            return result;
        }

        await _movieRepository.UpdateAsync(movie);

        return Result.Success();
    }
}