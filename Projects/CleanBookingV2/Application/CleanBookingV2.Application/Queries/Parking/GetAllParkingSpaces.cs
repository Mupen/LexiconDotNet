using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Application.Queries.Parking;

/// <summary>
/// Reads parking spaces for display.
/// The query maps domain entities to read models so the API can return stable DTOs
/// without exposing persistence or domain internals.
/// </summary>
public sealed class GetAllParkingSpaces
{
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public GetAllParkingSpaces(IParkingSpaceRepository parkingSpaceRepository)
    {
        _parkingSpaceRepository = parkingSpaceRepository;
    }

    /// <summary>
    /// Returns parking spaces ordered by stable id.
    /// Sorting in the query keeps controllers simple and gives the frontend a
    /// predictable list without needing client-side ordering rules.
    /// </summary>
    public async Task<IReadOnlyList<ParkingSpaceReadModel>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var spaces = await _parkingSpaceRepository.GetAllAsync(cancellationToken);

        return spaces
            .OrderBy(space => space.Id)
            .Select(space => new ParkingSpaceReadModel(
                space.Id,
                space.Name,
                space.ParkingSpaceType,
                space.IsActive))
            .ToList();
    }
}
