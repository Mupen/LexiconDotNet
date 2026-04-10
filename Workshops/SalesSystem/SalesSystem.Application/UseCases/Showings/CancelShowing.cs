using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Showings;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Showings;

public sealed class CancelShowing
{
    private readonly IShowingRepository _showingRepository;

    public CancelShowing(IShowingRepository showingRepository)
    {
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
    }

    public async Task<Result> ExecuteAsync(CancelShowingRequest request)
    {
        var showing = await _showingRepository.GetByIdAsync(request.ShowingId);

        if (showing is null)
        {
            return Result.Failure(
                new Error("Showing.NotFound", "Showing was not found."));
        }

        var result = showing.Cancel();

        if (result.IsFailure)
        {
            return result;
        }

        await _showingRepository.UpdateAsync(showing);

        return Result.Success();
    }
}