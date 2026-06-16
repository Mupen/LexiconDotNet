using CleanBookingV1.Domain.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Parking;

public sealed class GetAllParkingSpaces
{
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public GetAllParkingSpaces(IParkingSpaceRepository parkingSpaceRepository)
    {
        _parkingSpaceRepository = parkingSpaceRepository
            ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));
    }

    public async Task<Result<IReadOnlyList<ParkingSpace>>> ExecuteAsync()
    {
        var parkingSpaces = await _parkingSpaceRepository.GetAllAsync();
        return Result<IReadOnlyList<ParkingSpace>>.Success(parkingSpaces);
    }
}