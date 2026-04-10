using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Interfaces;

public interface ITicketOrderRepository
{
    Task<IReadOnlyList<TicketOrder>> GetAllAsync();
    Task<TicketOrder?> GetByIdAsync(Guid id);
    Task AddAsync(TicketOrder ticketOrder);
    Task UpdateAsync(TicketOrder ticketOrder);
    Task DeleteAsync(Guid id);

    // Task<IReadOnlyList<TicketOrder>> GetByShowingIdAsync(Guid showingId);
}