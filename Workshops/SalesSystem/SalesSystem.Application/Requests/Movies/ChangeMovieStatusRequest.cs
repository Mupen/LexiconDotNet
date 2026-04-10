namespace SalesSystem.Application.Requests.Movies;

public sealed record ChangeMovieStatusRequest(
    Guid MovieId,
    bool IsActive);