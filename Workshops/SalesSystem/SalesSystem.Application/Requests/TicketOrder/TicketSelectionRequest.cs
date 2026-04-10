namespace SalesSystem.Application.Requests.TicketOrders;

public sealed record TicketSelectionRequest(
    int SeatNumber,
    int CustomerAge,
    bool CustomerIsPresent,
    bool HasAccompanyingAdult);