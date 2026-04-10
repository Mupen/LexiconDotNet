using System;
using System.Collections.Generic;

namespace SalesSystem.Application.Requests.TicketOrders;

public sealed record UpdateTicketOrderRequest(
    Guid TicketOrderId,
    IReadOnlyList<TicketSelectionRequest> Selections,
    string? CampaignCode);