using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Showings;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Showings;

public sealed class RestoreShowing
{
    private readonly IShowingRepository _showingRepository;

    public RestoreShowing(IShowingRepository showingRepository)
    {
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
    }

    public async Task<Result> ExecuteAsync(RestoreShowingRequest request)
    {
        var showing = await _showingRepository.GetByIdAsync(request.ShowingId);

        if (showing is null)
        {
            return Result.Failure(
                new Error("Showing.NotFound", "Showing was not found."));
        }

        var result = showing.Restore();

        if (result.IsFailure)
        {
            return result;
        }

        await _showingRepository.UpdateAsync(showing);

        return Result.Success();
    }
}