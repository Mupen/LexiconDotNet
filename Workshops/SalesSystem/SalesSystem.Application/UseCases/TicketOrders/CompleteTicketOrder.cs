using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.TicketOrders;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.TicketOrders;

public sealed class CompleteTicketOrder
{
    private readonly ITicketOrderRepository _ticketOrderRepository;

    public CompleteTicketOrder(ITicketOrderRepository ticketOrderRepository)
    {
        _ticketOrderRepository = ticketOrderRepository
            ?? throw new ArgumentNullException(nameof(ticketOrderRepository));
    }

    public async Task<Result> ExecuteAsync(CompleteTicketOrderRequest request)
    {
        var order = await _ticketOrderRepository.GetByIdAsync(request.TicketOrderId);

        if (order is null)
        {
            return Result.Failure(
                new Error("TicketOrder.NotFound", "Ticket order was not found."));
        }

        var result = order.Complete();

        if (result.IsFailure)
        {
            return result;
        }

        await _ticketOrderRepository.UpdateAsync(order);

        return Result.Success();
    }
}