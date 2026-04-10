using SalesSystem.Domain.Enums;

namespace SalesSystem.Application.Requests.Movies;

public sealed record UpdateMovieRequest(
    Guid MovieId,
    int MovieNumber,
    string Title,
    string Description,
    int YearReleased,
    AgeRating AgeRating,
    TimeSpan Duration);