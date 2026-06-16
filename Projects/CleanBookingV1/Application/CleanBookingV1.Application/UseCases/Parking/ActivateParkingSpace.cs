using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Parking;
using CleanBookingV1.Domain.Contracts;

namespace CleanBookingV1.Application.UseCases.Parking;

public sealed class ActivateParkingSpace
{
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public ActivateParkingSpace(IParkingSpaceRepository parkingSpaceRepository)
    {
        _parkingSpaceRepository = parkingSpaceRepository
            ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));
    }

    public async Task<Result> ExecuteAsync(ActivateParkingSpaceRequest? request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("ParkingSpace.Activate.NullRequest", "Request cannot be null."));
        }

        if (request.ParkingSpaceId <= 0)
        {
            return Result.Failure(
                new Error("ParkingSpace.Activate.InvalidId", "Parking space id must be greater than zero."));
        }

        var parkingSpace = await _parkingSpaceRepository.GetByIdAsync(request.ParkingSpaceId);

        if (parkingSpace is null)
        {
            return Result.Failure(
                new Error("ParkingSpace.Activate.NotFound", "The parking space was not found."));
        }

        parkingSpace.Activate();

        await _parkingSpaceRepository.UpdateAsync(parkingSpace);

        return Result.Success();
    }
}