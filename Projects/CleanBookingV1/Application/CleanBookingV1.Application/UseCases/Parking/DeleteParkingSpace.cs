using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests;
using CleanBookingV1.Application.Requests.Parking;
using CleanBookingV1.Domain.Contracts;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Parking;

public sealed class DeleteParkingSpace
{
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public DeleteParkingSpace(IParkingSpaceRepository parkingSpaceRepository)
    {
        _parkingSpaceRepository = parkingSpaceRepository
            ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));
    }

    public async Task<Result> ExecuteAsync(DeleteParkingSpaceRequest? request)
    {

        var validationResult = ParkingSpaceRequestValidator.Validate(request);

        if (validationResult.IsFailure)
        {
            return Result<ParkingSpace>.Failure(validationResult.Error);
        }

        var parkingSpace = await _parkingSpaceRepository.GetByIdAsync(request!.ParkingSpaceId);

        if (parkingSpace is null)
        {
            return Result.Failure(
                new Error("ParkingSpace.Delete.NotFound", "The parking space was not found."));
        }

        await _parkingSpaceRepository.DeleteAsync(request.ParkingSpaceId);

        return Result.Success();
    }
}