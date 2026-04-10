using System;

namespace SalesSystem.Application.Requests.Showings;

public sealed record CreateShowingRequest(
    Guid MovieId,
    DateOnly Date,
    TimeOnly StartTime,
    int SeatCount);