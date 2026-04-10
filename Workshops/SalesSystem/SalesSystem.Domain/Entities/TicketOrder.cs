using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Enums;

namespace SalesSystem.Domain.Entities;

public sealed class TicketOrder
{
    // Identity and core properties
    public Guid Id { get; }
    public Guid ShowingId { get; private set; }
    public DateTime CreatedAt { get; }
    public IReadOnlyList<TicketOrderItem> Tickets => _tickets.AsReadOnly();
    public TicketOrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    private readonly List<TicketOrderItem> _tickets = [];

    private TicketOrder(Guid id, Guid showingId, DateTime createdAt, List<TicketOrderItem> tickets, TicketOrderStatus status, decimal totalAmount)
    {
        Id = id;
        ShowingId = showingId;
        CreatedAt = createdAt;
        _tickets = tickets;
        Status = status;
        TotalAmount = totalAmount;
    }

    // Public operations
    public static Result<TicketOrder> Create(Guid showingId, List<TicketOrderItem> tickets)
    {
        Result result;

        result = ValidateShowingId(showingId);
        if (result.IsFailure)
            return Result<TicketOrder>.Failure(result.Error);

        result = ValidateTickets(tickets);
        if (result.IsFailure)
            return Result<TicketOrder>.Failure(result.Error);

        var totalAmount = CalculateTotalAmount(tickets);

        var ticketOrder = new TicketOrder(
            Guid.NewGuid(),
            showingId,
            DateTime.Now,
            new List<TicketOrderItem>(tickets),
            TicketOrderStatus.Active,
            totalAmount);

        return Result<TicketOrder>.Success(ticketOrder);
    }

    public Result Update(List<TicketOrderItem> tickets)
    {
        var result = ValidateTicketsChange(tickets);
        if (result.IsFailure)
            return result;

        ApplyTicketsChange(tickets);

        return Result.Success();
    }

    public Result Cancel()
    {
        var result = ValidateCancel();
        if (result.IsFailure)
            return result;

        ApplyCancel();

        return Result.Success();
    }

    public Result Complete()
    {
        var result = ValidateComplete();
        if (result.IsFailure)
            return result;

        ApplyComplete();

        return Result.Success();
    }

    // ShowingId operations
    private static Result ValidateShowingId(Guid showingId)
    {
        if (showingId == Guid.Empty)
        {
            return Result.Failure(
                new Error("TicketOrder.InvalidShowingId", "Showing id is required."));
        }

        return Result.Success();
    }

    // Ticket operations
    private Result ValidateTicketsChange(List<TicketOrderItem> tickets)
    {
        return ValidateTickets(tickets);
    }

    private static Result ValidateTickets(List<TicketOrderItem> tickets)
    {
        if (tickets is null || tickets.Count == 0)
        {
            return Result.Failure(
                new Error("TicketOrder.InvalidTickets", "At least one ticket is required."));
        }

        return Result.Success();
    }

    private void ApplyTicketsChange(List<TicketOrderItem> tickets)
    {
        _tickets.Clear();
        _tickets.AddRange(tickets);
        TotalAmount = CalculateTotalAmount(_tickets);
    }

    private static decimal CalculateTotalAmount(IEnumerable<TicketOrderItem> tickets)
    {
        return tickets.Sum(ticket => ticket.Price);
    }

    // Status operations
    private Result ValidateCancel()
    {
        if (Status == TicketOrderStatus.Cancelled)
        {
            return Result.Failure(
                new Error("TicketOrder.AlreadyCancelled", "Order is already cancelled."));
        }

        if (Status == TicketOrderStatus.Completed)
        {
            return Result.Failure(
                new Error("TicketOrder.AlreadyCompleted", "Completed orders cannot be cancelled."));
        }

        return Result.Success();
    }

    private void ApplyCancel()
    {
        Status = TicketOrderStatus.Cancelled;
    }

    private Result ValidateComplete()
    {
        if (Status == TicketOrderStatus.Completed)
        {
            return Result.Failure(
                new Error("TicketOrder.AlreadyCompleted", "Order is already completed."));
        }

        if (Status == TicketOrderStatus.Cancelled)
        {
            return Result.Failure(
                new Error("TicketOrder.CancelledOrder", "Cancelled orders cannot be completed."));
        }

        return Result.Success();
    }

    private void ApplyComplete()
    {
        Status = TicketOrderStatus.Completed;
    }
}