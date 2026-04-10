using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Queries.Showings;

public sealed class GetShowingById
{
    private readonly IShowingRepository _showingRepository;

    public GetShowingById(IShowingRepository showingRepository)
    {
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
    }

    public async Task<Result<Showing>> ExecuteAsync(Guid showingId)
    {
        var showing = await _showingRepository.GetByIdAsync(showingId);

        if (showing is null)
        {
            return Result<Showing>.Failure(
                new Error("Showing.NotFound", "Showing was not found."));
        }

        return Result<Showing>.Success(showing);
    }
}