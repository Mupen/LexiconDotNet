namespace SalesSystem.Application.ReadModels.Showings;

public sealed record ShowingListItem(
    Guid ShowingId,
    int MovieNumber,
    string MovieTitle,
    int YearReleased,
    string Description,
    string AgeRating,
    TimeSpan Duration,
    DateOnly Date,
    TimeOnly StartTime,
    int AvailableSeats,
    bool IsCancelled);