using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Showings;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.Showings;

public sealed class UpdateShowing
{
    private readonly IShowingRepository _showingRepository;

    public UpdateShowing(IShowingRepository showingRepository)
    {
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
    }

    public async Task<Result> ExecuteAsync(UpdateShowingRequest request)
    {
        var showing = await _showingRepository.GetByIdAsync(request.ShowingId);

        if (showing is null)
        {
            return Result.Failure(
                new Error("Showing.NotFound", "Showing was not found."));
        }

        var result = showing.Update(
            request.MovieId,
            request.Date,
            request.StartTime);

        if (result.IsFailure)
        {
            return result;
        }

        await _showingRepository.UpdateAsync(showing);

        return Result.Success();
    }
}