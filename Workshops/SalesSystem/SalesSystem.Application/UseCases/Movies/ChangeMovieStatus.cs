using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Movies;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Movies;

public sealed class ChangeMovieStatus
{
    private readonly IMovieRepository _movieRepository;

    public ChangeMovieStatus(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<Result> ExecuteAsync(ChangeMovieStatusRequest request)
    {
        var movie = await _movieRepository.GetByIdAsync(request.MovieId);

        if (movie is null)
        {
            return Result.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        Result result = Result.Success();

        if (request.IsActive && !movie.IsActive)
        {
            result = movie.Activate();
        }
        else if (!request.IsActive && movie.IsActive)
        {
            result = movie.Deactivate();
        }

        if (result.IsFailure)
        {
            return result;
        }

        await _movieRepository.UpdateAsync(movie);

        return Result.Success();
    }
}