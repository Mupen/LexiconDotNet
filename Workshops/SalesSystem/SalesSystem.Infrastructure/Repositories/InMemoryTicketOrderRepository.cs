using SalesSystem.Application.Interfaces;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Infrastructure.Repositories;

public sealed class InMemoryTicketOrderRepository : ITicketOrderRepository
{
    private readonly List<TicketOrder> _ticketOrders = [];

    public Task<IReadOnlyList<TicketOrder>> GetAllAsync()
    {
        IReadOnlyList<TicketOrder> result = _ticketOrders.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<TicketOrder?> GetByIdAsync(Guid id)
    {
        TicketOrder? ticketOrder = _ticketOrders.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(ticketOrder);
    }

    public Task AddAsync(TicketOrder ticketOrder)
    {
        ArgumentNullException.ThrowIfNull(ticketOrder);

        _ticketOrders.Add(ticketOrder);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TicketOrder ticketOrder)
    {
        ArgumentNullException.ThrowIfNull(ticketOrder);

        int index = _ticketOrders.FindIndex(existing => existing.Id == ticketOrder.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"TicketOrder with id '{ticketOrder.Id}' was not found.");
        }

        _ticketOrders[index] = ticketOrder;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        TicketOrder? ticketOrder = _ticketOrders.FirstOrDefault(x => x.Id == id);

        if (ticketOrder is null)
        {
            throw new InvalidOperationException(
                $"TicketOrder with id '{id}' was not found.");
        }

        _ticketOrders.Remove(ticketOrder);
        return Task.CompletedTask;
    }
}