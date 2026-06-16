using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Parking;
using CleanBookingV1.Domain.Contracts;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Parking;

public sealed class UpdateParkingSpace
{
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public UpdateParkingSpace(IParkingSpaceRepository parkingSpaceRepository)
    {
        _parkingSpaceRepository = parkingSpaceRepository
            ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));
    }

    public async Task<Result<ParkingSpace>> ExecuteAsync(UpdateParkingSpaceRequest request)
    {
        if (request is null)
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.Update.NullRequest", "Request cannot be null."));
        }

        if (request.ParkingSpaceId <= 0)
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.Update.InvalidId", "Parking space id must be greater than zero."));
        }

        ParkingSpace? parkingSpace =
            await _parkingSpaceRepository.GetByIdAsync(request.ParkingSpaceId);

        if (parkingSpace is null)
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.Update.NotFound", "The parking space was not found."));
        }

        Result updateResult = parkingSpace.UpdateDetails(
            name: request.Name,
            type: request.Type);

        if (updateResult.IsFailure)
        {
            return Result<ParkingSpace>.Failure(updateResult.Error);
        }

        await _parkingSpaceRepository.UpdateAsync(parkingSpace);

        return Result<ParkingSpace>.Success(parkingSpace);
    }
}