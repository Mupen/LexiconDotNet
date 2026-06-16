using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Application.Queries.Rooms;

/// <summary>
/// Reads rooms for display.
/// It maps domain entities into read models so API responses are not tied directly
/// to mutable domain objects.
/// </summary>
public sealed class GetAllRooms
{
    private readonly IRoomRepository _roomRepository;

    public GetAllRooms(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Returns rooms ordered by stable id.
    /// The ordering is applied here because it is a presentation-friendly query
    /// concern and keeps controllers from repeating sort logic.
    /// </summary>
    public async Task<IReadOnlyList<RoomReadModel>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var rooms = await _roomRepository.GetAllAsync(cancellationToken);

        return rooms
            .OrderBy(room => room.Id)
            .Select(room => new RoomReadModel(
                room.Id,
                room.Name,
                room.RoomType,
                room.SizeInSquareMeters,
                room.Capacity,
                room.PricePerNight,
                room.IsActive))
            .ToList();
    }
}
