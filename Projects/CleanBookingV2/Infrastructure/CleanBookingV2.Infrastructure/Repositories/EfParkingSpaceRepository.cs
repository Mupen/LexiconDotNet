using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for parking space domain entities.
/// Booking preparation uses this repository to validate optional parking choices
/// against backend-owned data.
/// </summary>
public sealed class EfParkingSpaceRepository : IParkingSpaceRepository
{
    private readonly CleanBookingV2DbContext _dbContext;

    public EfParkingSpaceRepository(CleanBookingV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns all parking spaces ordered by id without tracking.
    /// The data is read-only in current workflows, so EF change tracking would add
    /// overhead without value.
    /// </summary>
    public async Task<IReadOnlyList<ParkingSpace>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingSpaces
            .AsNoTracking()
            .OrderBy(space => space.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns one parking space by id or null when it does not exist.
    /// Null is handled by the application layer as a business failure instead of an
    /// infrastructure exception.
    /// </summary>
    public async Task<ParkingSpace?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingSpaces
            .AsNoTracking()
            .FirstOrDefaultAsync(space => space.Id == id, cancellationToken);
    }
}
