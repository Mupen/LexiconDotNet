using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.TicketOrders;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;
using SalesSystem.Domain.Interfaces;

namespace SalesSystem.Application.UseCases.TicketOrders;

public sealed class SellTickets
{
    private readonly IShowingRepository _showingRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ITicketOrderRepository _ticketOrderRepository;
    private readonly ITicketPricingService _ticketPricingService;

    public SellTickets(
        IMovieRepository movieRepository,
        IShowingRepository showingRepository,
        ITicketOrderRepository ticketOrderRepository,
        ITicketPricingService ticketPricingService)
    {
        _showingRepository = showingRepository
            ?? throw new ArgumentNullException(nameof(showingRepository));
        _movieRepository = movieRepository
            ?? throw new ArgumentNullException(nameof(movieRepository));
        _ticketOrderRepository = ticketOrderRepository
            ?? throw new ArgumentNullException(nameof(ticketOrderRepository));
        _ticketPricingService = ticketPricingService
            ?? throw new ArgumentNullException(nameof(ticketPricingService));
    }

    public async Task<Result<TicketOrder>> ExecuteAsync(SellTicketsRequest request)
    {
        // ------------------------------------------------------------
        // 1. Load and validate the showing.
        // Why: We cannot sell tickets unless the showing exists and is active.
        // ------------------------------------------------------------
        var showing = await GetValidShowingAsync(request.ShowingId);
        if (showing.IsFailure)
            return showing.ToFailure<TicketOrder>();

        // ------------------------------------------------------------
        // 2. Load the movie connected to the showing.
        // Why: Ticket pricing depends on the movie age rating.
        // ------------------------------------------------------------
        var movie = await GetMovieForShowingAsync(showing.Value.MovieId);
        if (movie.IsFailure)
            return movie.ToFailure<TicketOrder>();

        // ------------------------------------------------------------
        // 3. Validate the ticket selections before processing seats.
        // Why: We fail early on empty or duplicate input before changing state.
        // ------------------------------------------------------------
        var selectionValidation = ValidateSelections(request.Selections);
        if (selectionValidation.IsFailure)
            return selectionValidation.ToFailure<TicketOrder>();

        // ------------------------------------------------------------
        // 4. Build shared lookup data used during ticket processing.
        // Why: We calculate these once instead of repeating the same work
        // for every selected seat.
        // ------------------------------------------------------------
        bool hasAtLeastOnePayingCustomerInOrder = HasAtLeastOnePayingCustomer(request.Selections);
        var seatsByNumber = showing.Value.Seats.ToDictionary(s => s.SeatNumber);
        var tickets = new List<TicketOrderItem>();

        // ------------------------------------------------------------
        // 5. Process each seat selection.
        // Why: For each requested seat we validate availability, calculate
        // ticket price, reserve the seat, create the ticket, and mark
        // the seat as sold.
        // ------------------------------------------------------------
        foreach (var selection in request.Selections)
        {
            var ticketResult = ProcessSelection(
                movie.Value,
                showing.Value,
                selection,
                hasAtLeastOnePayingCustomerInOrder,
                request.CampaignCode,
                seatsByNumber);

            if (ticketResult.IsFailure)
                return Result<TicketOrder>.Failure(ticketResult.Error);

            tickets.Add(ticketResult.Value);
        }

        // ------------------------------------------------------------
        // 6. Create and persist the ticket order.
        // Why: Once all selected seats have been processed successfully, we
        // create the final order and save both the showing and the order.
        // ------------------------------------------------------------
        var orderResult = TicketOrder.Create(showing.Value.Id, tickets);
        if (orderResult.IsFailure)
            return orderResult;

        await _showingRepository.UpdateAsync(showing.Value);
        await _ticketOrderRepository.AddAsync(orderResult.Value);

        return orderResult;
    }

    private async Task<Result<Showing>> GetValidShowingAsync(Guid showingId)
    {
        var showing = await _showingRepository.GetByIdAsync(showingId);

        if (showing is null)
        {
            return Result<Showing>.Failure(
                new Error("Showing.NotFound", "Showing was not found."));
        }

        if (showing.IsCancelled)
        {
            return Result<Showing>.Failure(
                new Error("Showing.Cancelled", "Cannot sell tickets for a cancelled showing."));
        }

        return Result<Showing>.Success(showing);
    }

    private async Task<Result<Movie>> GetMovieForShowingAsync(Guid movieId)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);

        if (movie is null)
        {
            return Result<Movie>.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        return Result<Movie>.Success(movie);
    }

    private static Result ValidateSelections(IReadOnlyList<TicketSelectionRequest> selections)
    {
        if (selections.Count == 0)
        {
            return Result.Failure(
                new Error("TicketOrder.NoSeats", "At least one seat must be selected."));
        }

        var duplicateSeats = selections
            .GroupBy(s => s.SeatNumber)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateSeats.Count > 0)
        {
            return Result.Failure(
                new Error("Seat.Duplicate", $"Duplicate seat selection: {string.Join(", ", duplicateSeats)}"));
        }

        return Result.Success();
    }

    private static bool HasAtLeastOnePayingCustomer(IReadOnlyList<TicketSelectionRequest> selections)
    {
        return selections.Any(s => s.CustomerAge >= 6 || !s.CustomerIsPresent);
    }

    private Result<TicketOrderItem> ProcessSelection(Movie movie, Showing showing, TicketSelectionRequest selection, bool hasAtLeastOnePayingCustomerInOrder, string? campaignCode, IReadOnlyDictionary<int, ShowingSeat> seatsByNumber)
    {
        if (!seatsByNumber.TryGetValue(selection.SeatNumber, out var seat))
        {
            return Result<TicketOrderItem>.Failure(
                new Error("Seat.NotFound", $"Seat {selection.SeatNumber} does not exist."));
        }

        if (!seat.IsAvailable)
        {
            return Result<TicketOrderItem>.Failure(
                new Error("Seat.NotAvailable", $"Seat {seat.SeatNumber} is not available."));
        }

        var priceResult = _ticketPricingService.CalculatePrice(
            movie,
            showing,
            selection.CustomerAge,
            selection.CustomerIsPresent,
            hasAtLeastOnePayingCustomerInOrder,
            selection.HasAccompanyingAdult,
            campaignCode);

        if (priceResult.IsFailure)
            return Result<TicketOrderItem>.Failure(priceResult.Error);

        var reserveResult = seat.Reserve();
        if (reserveResult.IsFailure)
            return Result<TicketOrderItem>.Failure(reserveResult.Error);

        var ticketResult = TicketOrderItem.Create(seat.Id, priceResult.Value);
        if (ticketResult.IsFailure)
        {
            seat.Release();
            return ticketResult;
        }

        var sellResult = seat.Sell();
        if (sellResult.IsFailure)
        {
            seat.Release();
            return Result<TicketOrderItem>.Failure(sellResult.Error);
        }

        return ticketResult;
    }
}