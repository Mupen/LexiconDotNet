using System;

namespace SalesSystem.Application.Requests.Showings;

public sealed record CancelShowingRequest(
    Guid ShowingId);