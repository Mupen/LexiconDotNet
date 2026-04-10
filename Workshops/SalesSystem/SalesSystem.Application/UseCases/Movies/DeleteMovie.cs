using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Movies;

public sealed class DeleteMovie
{
    private readonly IMovieRepository _movieRepository;

    public DeleteMovie(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<Result> ExecuteAsync(Guid movieId)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);

        if (movie is null)
        {
            return Result.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        await _movieRepository.DeleteAsync(movieId);

        return Result.Success();
    }
}