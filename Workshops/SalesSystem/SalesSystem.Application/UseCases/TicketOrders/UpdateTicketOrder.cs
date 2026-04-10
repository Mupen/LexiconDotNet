using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.TicketOrders;
using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;
using SalesSystem.Domain.Interfaces;
using SalesSystem.Domain.Enums;

namespace SalesSystem.Application.UseCases.TicketOrders;

public sealed class UpdateTicketOrder
{
    private readonly IMovieRepository _movieRepository;
    private readonly IShowingRepository _showingRepository;
    private readonly ITicketOrderRepository _ticketOrderRepository;
    private readonly ITicketPricingService _ticketPricingService;

    public UpdateTicketOrder(
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

    public async Task<Result> ExecuteAsync(UpdateTicketOrderRequest request)
    {
        // ------------------------------------------------------------
        // 1. Load and validate the order.
        // Why:
        // Only existing active orders can be updated.
        // ------------------------------------------------------------
        var order = await _ticketOrderRepository.GetByIdAsync(request.TicketOrderId);

        if (order is null)
        {
            return Result.Failure(
                new Error("TicketOrder.NotFound", "Ticket order was not found."));
        }

        if (order.Status != TicketOrderStatus.Active)
        {
            return Result.Failure(
                new Error("TicketOrder.NotActive", "Only active ticket orders can be updated."));
        }

        // ------------------------------------------------------------
        // 2. Load and validate the showing.
        // Why:
        // Updated tickets must still belong to a valid active showing.
        // ------------------------------------------------------------
        var showing = await _showingRepository.GetByIdAsync(order.ShowingId);

        if (showing is null)
        {
            return Result.Failure(
                new Error("Showing.NotFound", "Showing was not found."));
        }

        if (showing.IsCancelled)
        {
            return Result.Failure(
                new Error("Showing.Cancelled", "Cannot update tickets for a cancelled showing."));
        }

        // ------------------------------------------------------------
        // 3. Load the movie connected to the showing.
        // Why:
        // Ticket pricing depends on movie age rating rules.
        // ------------------------------------------------------------
        var movie = await _movieRepository.GetByIdAsync(showing.MovieId);

        if (movie is null)
        {
            return Result.Failure(
                new Error("Movie.NotFound", "Movie was not found."));
        }

        // ------------------------------------------------------------
        // 4. Validate the updated selections.
        // Why:
        // We fail early on empty or duplicate seat input before doing
        // any ticket recalculation.
        // ------------------------------------------------------------
        if (request.Selections is null || request.Selections.Count == 0)
        {
            return Result.Failure(
                new Error("TicketOrder.NoSeats", "At least one seat must be selected."));
        }

        var duplicateSeats = request.Selections
            .GroupBy(s => s.SeatNumber)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateSeats.Count > 0)
        {
            return Result.Failure(
                new Error("Seat.Duplicate", $"Duplicate seat selection: {string.Join(", ", duplicateSeats)}"));
        }

        // ------------------------------------------------------------
        // 5. Build shared lookup data.
        // Why:
        // We calculate shared values once instead of repeating work
        // for every selected seat.
        // ------------------------------------------------------------
        var currentSeatIds = order.Tickets
            .Select(t => t.ShowingSeatId)
            .ToHashSet();

        bool hasAtLeastOnePayingCustomerInOrder = request.Selections
            .Any(s => s.CustomerAge >= 6 || !s.CustomerIsPresent);

        var seatsByNumber = showing.Seats.ToDictionary(s => s.SeatNumber);

        var updatedTickets = new List<TicketOrderItem>();

        // ------------------------------------------------------------
        // 6. Recalculate all requested tickets.
        // Why:
        // Each updated seat must exist, be allowed for this order, and
        // have its price recalculated using the latest rules.
        // ------------------------------------------------------------
        foreach (var selection in request.Selections)
        {
            if (!seatsByNumber.TryGetValue(selection.SeatNumber, out var seat))
            {
                return Result.Failure(
                    new Error("Seat.NotFound", $"Seat {selection.SeatNumber} does not exist."));
            }

            var seatAlreadyBelongsToOrder = currentSeatIds.Contains(seat.Id);

            if (!seatAlreadyBelongsToOrder && !seat.IsAvailable)
            {
                return Result.Failure(
                    new Error("Seat.NotAvailable", $"Seat {seat.SeatNumber} is not available."));
            }

            var priceResult = _ticketPricingService.CalculatePrice(
                movie,
                showing,
                selection.CustomerAge,
                selection.CustomerIsPresent,
                hasAtLeastOnePayingCustomerInOrder,
                selection.HasAccompanyingAdult,
                request.CampaignCode);

            if (priceResult.IsFailure)
            {
                return priceResult;
            }

            var ticketResult = TicketOrderItem.Create(seat.Id, priceResult.Value);

            if (ticketResult.IsFailure)
            {
                return ticketResult;
            }

            updatedTickets.Add(ticketResult.Value);
        }

        // ------------------------------------------------------------
        // 7. Apply and persist the updated order.
        // Why:
        // Once all recalculated tickets are valid, we replace the
        // order contents and save the updated order.
        // ------------------------------------------------------------
        var updateResult = order.Update(updatedTickets);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await _ticketOrderRepository.UpdateAsync(order);

        return Result.Success();
    }
}