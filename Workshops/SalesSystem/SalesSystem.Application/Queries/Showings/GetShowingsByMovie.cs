using SalesSystem.Application.Interfaces;
using SalesSystem.Application.ReadModels.Showings;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Extensions;

namespace SalesSystem.Application.Queries.Showings;

public sealed class GetShowingsByMovie
{
    private readonly IMovieRepository _movieRepository;
    private readonly IShowingRepository _showingRepository;

    public GetShowingsByMovie(
        IMovieRepository movieRepository,
        IShowingRepository showingRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
    }

    public async Task<Result<IReadOnlyList<ShowingListItem>>> ExecuteAsync(Guid movieId)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);

        if (movie is null)
        {
            return Result<IReadOnlyList<ShowingListItem>>.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        var showings = await _showingRepository.GetByMovieIdAsync(movieId);

        var items = showings
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .Select(s => new ShowingListItem(
                ShowingId: s.Id,
                MovieNumber: movie.MovieNumber,
                MovieTitle: movie.Title,
                YearReleased: movie.YearReleased,
                Description: movie.Description,
                AgeRating: movie.AgeRating.GetLabel(),
                Duration: movie.Duration,
                Date: s.Date,
                StartTime: s.StartTime,
                AvailableSeats: s.Seats.Count(seat => seat.IsAvailable),
                IsCancelled: s.IsCancelled))
            .ToList();

        return Result<IReadOnlyList<ShowingListItem>>.Success(items);
    }
}