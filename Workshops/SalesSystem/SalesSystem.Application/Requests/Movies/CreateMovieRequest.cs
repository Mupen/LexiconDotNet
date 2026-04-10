using SalesSystem.Domain.Enums;

namespace SalesSystem.Application.Requests.Movies;

public sealed record CreateMovieRequest(
    int MovieNumber,
    string Title,
    string Description,
    int YearReleased,
    AgeRating AgeRating,
    TimeSpan Duration,
    bool IsActive = true);