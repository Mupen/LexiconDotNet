namespace SalesSystem.Application.Requests.TicketOrders;

public sealed record SellTicketsRequest(
    Guid ShowingId,
    IReadOnlyList<TicketSelectionRequest> Selections,
    string? CampaignCode);