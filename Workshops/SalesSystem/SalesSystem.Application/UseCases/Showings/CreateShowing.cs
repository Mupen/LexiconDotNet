using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.Showings;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.UseCases.Showings;

public sealed class CreateShowing
{
    private readonly IShowingRepository _showingRepository;

    public CreateShowing(IShowingRepository showingRepository)
    {
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
    }

    public async Task<Result<Showing>> ExecuteAsync(CreateShowingRequest request)
    {
        var result = Showing.Create(
            request.MovieId,
            request.Date,
            request.StartTime,
            request.SeatCount);

        if (result.IsFailure)
        {
            return result;
        }

        await _showingRepository.AddAsync(result.Value);

        return result;
    }
}