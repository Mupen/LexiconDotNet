using SalesSystem.Application.Interfaces;
using SalesSystem.Application.ReadModels.Showings;
using SalesSystem.Domain.Extensions;

namespace SalesSystem.Application.Queries.Showings;

public sealed class GetAvailableShowings
{
    private readonly IMovieRepository _movieRepository;
    private readonly IShowingRepository _showingRepository;
    

    public GetAvailableShowings(
        IMovieRepository movieRepository,
        IShowingRepository showingRepository)
    {
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
    }

    public async Task<IReadOnlyList<ShowingListItem>> ExecuteAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        var showings = await _showingRepository.GetAllAsync();

        var moviesById = movies.ToDictionary(m => m.Id);

        var items = showings
            .Where(s => !s.IsCancelled)
            .Where(s => moviesById.ContainsKey(s.MovieId))
            .Select(s =>
            {
                var movie = moviesById[s.MovieId];

                return new ShowingListItem(
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
                    IsCancelled: s.IsCancelled);
            })
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToList();

        return items;
    }
}