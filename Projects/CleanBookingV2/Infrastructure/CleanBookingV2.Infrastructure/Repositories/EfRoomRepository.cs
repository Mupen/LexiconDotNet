using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for room domain entities.
/// Rooms are read as authoritative backend data for capacity, price, and active
/// status; the frontend must not be trusted for these values.
/// </summary>
public sealed class EfRoomRepository : IRoomRepository
{
    private readonly CleanBookingV2DbContext _dbContext;

    public EfRoomRepository(CleanBookingV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns all rooms ordered by id without tracking.
    /// No tracking is needed because this repository currently supports reads only
    /// for rooms.
    /// </summary>
    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .OrderBy(room => room.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns one room by id or null when it does not exist.
    /// Booking preparation uses this to validate that the selected room id from the
    /// frontend refers to real backend data.
    /// </summary>
    public async Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(room => room.Id == id, cancellationToken);
    }
}
