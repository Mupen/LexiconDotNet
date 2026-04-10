using System;

namespace SalesSystem.Application.Requests.TicketOrders;

public sealed record CompleteTicketOrderRequest(
    Guid TicketOrderId);