using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Requests.TicketOrders;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Application.UseCases.TicketOrders;

public sealed class CancelTicketOrder
{
    private readonly ITicketOrderRepository _ticketOrderRepository;

    public CancelTicketOrder(ITicketOrderRepository ticketOrderRepository)
    {
        _ticketOrderRepository = ticketOrderRepository
            ?? throw new ArgumentNullException(nameof(ticketOrderRepository));
    }

    public async Task<Result> ExecuteAsync(CancelTicketOrderRequest request)
    {
        var order = await _ticketOrderRepository.GetByIdAsync(request.TicketOrderId);

        if (order is null)
        {
            return Result.Failure(
                new Error("TicketOrder.NotFound", "Ticket order was not found."));
        }

        var result = order.Cancel();

        if (result.IsFailure)
        {
            return result;
        }

        await _ticketOrderRepository.UpdateAsync(order);

        return Result.Success();
    }
}