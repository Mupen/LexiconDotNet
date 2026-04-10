using System;

namespace SalesSystem.Application.Requests.Showings;

public sealed record UpdateShowingRequest(
    Guid ShowingId,
    Guid MovieId,
    DateOnly Date,
    TimeOnly StartTime);