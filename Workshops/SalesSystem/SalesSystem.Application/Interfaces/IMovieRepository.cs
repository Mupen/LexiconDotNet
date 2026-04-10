using SalesSystem.Domain.Entities;

namespace SalesSystem.Application.Interfaces;

public interface IMovieRepository
{
    Task<IReadOnlyList<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie?> GetMovieByNumberAsync(int movieNumber);
    Task AddAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(Guid id);
}