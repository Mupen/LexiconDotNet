using System;

namespace SalesSystem.Application.Requests.TicketOrders;

public sealed record CancelTicketOrderRequest(
    Guid TicketOrderId);