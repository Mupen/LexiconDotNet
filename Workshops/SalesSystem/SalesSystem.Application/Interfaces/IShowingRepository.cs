using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Interfaces;

public interface IShowingRepository
{
    Task<IReadOnlyList<Showing>> GetAllAsync();
    Task<Showing?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Showing>> GetByMovieIdAsync(Guid movieId);
    Task AddAsync(Showing showing);
    Task UpdateAsync(Showing showing);
    Task DeleteAsync(Guid id);
}