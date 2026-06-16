using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests;
using CleanBookingV1.Domain.Contracts;

namespace CleanBookingV1.Application.UseCases.Parking;

public sealed class DeactivateParkingSpace
{
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public DeactivateParkingSpace(IParkingSpaceRepository parkingSpaceRepository)
    {
        _parkingSpaceRepository = parkingSpaceRepository
            ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));
    }

    public async Task<Result> ExecuteAsync(DeactivateParkingSpaceRequest? request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("ParkingSpace.Deactivate.NullRequest", "Request cannot be null."));
        }

        if (request.ParkingSpaceId <= 0)
        {
            return Result.Failure(
                new Error("ParkingSpace.Deactivate.InvalidId", "Parking space id must be greater than zero."));
        }

        var parkingSpace = await _parkingSpaceRepository.GetByIdAsync(request.ParkingSpaceId);

        if (parkingSpace is null)
        {
            return Result.Failure(
                new Error("ParkingSpace.Deactivate.NotFound", "The parking space was not found."));
        }

        parkingSpace.Deactivate();

        await _parkingSpaceRepository.UpdateAsync(parkingSpace);

        return Result.Success();
    }
}