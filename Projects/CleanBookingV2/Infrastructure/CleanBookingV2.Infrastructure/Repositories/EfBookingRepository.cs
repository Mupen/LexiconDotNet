using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Infrastructure.Persistence;
using CleanBookingV2.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for write-oriented Booking access.
/// This repository returns domain entities for use cases that need to mutate state.
/// Read-only API screens use EfBookingReadRepository instead because projections
/// are more efficient and avoid tracking.
/// </summary>
public sealed class EfBookingRepository : IBookingRepository
{
    private readonly CleanBookingV2DbContext _dbContext;

    public EfBookingRepository(CleanBookingV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns bookings ordered by stay start without tracking.
    /// This method is mostly useful for domain-style reads; API list responses use
    /// the read repository to include joined display fields.
    /// </summary>
    public async Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .OrderBy(booking => booking.Stay.Start)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Loads one booking as a tracked entity for mutation.
    /// Tracking is intentional here because update and cancel use cases modify the
    /// returned entity and then call SaveChangesAsync.
    /// </summary>
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Bookings.FirstOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    /// <summary>
    /// Loads bookings for a room to support room availability checks.
    /// AsNoTracking is used because the availability service only asks each entity
    /// whether it overlaps; it does not modify these bookings.
    /// </summary>
    public async Task<IReadOnlyList<Booking>> GetByRoomIdAsync(int roomId, CancellationToken cancellationToken)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.RoomId == roomId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Loads bookings for a parking space to support parking availability checks.
    /// Keeping this query focused avoids loading unrelated bookings into memory.
    /// </summary>
    public async Task<IReadOnlyList<Booking>> GetByParkingSpaceIdAsync(int parkingSpaceId, CancellationToken cancellationToken)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.ParkingSpaceId == parkingSpaceId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new booking to the EF change tracker.
    /// The actual database insert happens on SaveChangesAsync so the use case can
    /// keep add and save inside the same transaction.
    /// </summary>
    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        await _dbContext.Bookings.AddAsync(booking, cancellationToken);
    }

    /// <summary>
    /// Persists tracked booking changes and translates EF concurrency exceptions.
    /// Use cases should not depend on EF Core exception types, so infrastructure
    /// converts DbUpdateConcurrencyException into BookingConcurrencyException.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BookingConcurrencyException();
        }
    }
}
