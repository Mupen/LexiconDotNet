using SalesSystem.Application.Interfaces;
using SalesSystem.Application.ReadModels.Showings;
using SalesSystem.Domain.Extensions;

namespace SalesSystem.Application.Queries.Showings;

public sealed class GetShowingsByDate
{
    private readonly IMovieRepository _movieRepository;
    private readonly IShowingRepository _showingRepository;

    public GetShowingsByDate(
        IMovieRepository movieRepository,
        IShowingRepository showingRepository)
    {
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public async Task<IReadOnlyList<ShowingListItem>> ExecuteAsync(DateOnly date)
    {
        var movies = await _movieRepository.GetAllAsync();
        var showings = await _showingRepository.GetAllAsync();

        var moviesById = movies.ToDictionary(m => m.Id);

        var items = showings
            .Where(s => s.Date == date)
            .Where(s => moviesById.ContainsKey(s.MovieId))
            .Select(s =>
            {
                var movie = moviesById[s.MovieId];

                return new
                {
                    Showing = s,
                    Movie = movie
                };
            })
            .Select(x => new ShowingListItem(
                ShowingId: x.Showing.Id,
                MovieNumber: x.Movie.MovieNumber,
                MovieTitle: x.Movie.Title,
                YearReleased: x.Movie.YearReleased,
                Description: x.Movie.Description,
                AgeRating: x.Movie.AgeRating.GetLabel(),
                Duration: x.Movie.Duration,
                Date: x.Showing.Date,
                StartTime: x.Showing.StartTime,
                AvailableSeats: x.Showing.Seats.Count(seat => seat.IsAvailable),
                IsCancelled: x.Showing.IsCancelled))
            .OrderBy(x => x.StartTime)
            .ToList();

        return items;
    }
}