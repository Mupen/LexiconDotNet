using SalesSystem.Domain.Contracts;

namespace SalesSystem.Domain.Entities;

public sealed class TicketOrderItem
{
    // Identity and core properties
    public Guid Id { get; }
    public Guid ShowingSeatId { get; private set; }
    public decimal Price { get; private set; }

    private TicketOrderItem(Guid id, Guid showingSeatId, decimal price)
    {
        Id = id;
        ShowingSeatId = showingSeatId;
        Price = price;
    }

    // Public operations
    public static Result<TicketOrderItem> Create(Guid showingSeatId, decimal price)
    {
        Result result;

        result = ValidateShowingSeatId(showingSeatId);
        if (result.IsFailure)
            return Result<TicketOrderItem>.Failure(result.Error);

        result = ValidatePrice(price);
        if (result.IsFailure)
            return Result<TicketOrderItem>.Failure(result.Error);

        var ticket = new TicketOrderItem(
            Guid.NewGuid(),
            showingSeatId,
            price);

        return Result<TicketOrderItem>.Success(ticket);
    }

    // ShowingSeatId operations
    private static Result ValidateShowingSeatId(Guid showingSeatId)
    {
        if (showingSeatId == Guid.Empty)
        {
            return Result.Failure(
                new Error("Ticket.InvalidShowingSeatId", "Showing seat id is required."));
        }

        return Result.Success();
    }

    // Price operations
    private static Result ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            return Result.Failure(
                new Error("Ticket.InvalidPrice", "Price cannot be negative."));
        }

        return Result.Success();
    }
}