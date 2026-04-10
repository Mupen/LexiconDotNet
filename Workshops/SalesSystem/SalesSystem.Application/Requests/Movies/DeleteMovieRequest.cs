namespace SalesSystem.Application.Requests.Movies;

public sealed record DeleteMovieRequest(
    Guid MovieId);