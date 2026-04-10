using System;

namespace SalesSystem.Application.Requests.Showings;

public sealed record RestoreShowingRequest(
    Guid ShowingId);