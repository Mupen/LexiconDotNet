using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Parking;
using CleanBookingV1.Domain.Contracts;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Parking;

public sealed class CreateParkingSpace
{
    private readonly IParkingSpaceRepository _parkingSpaceRepository;
    private readonly IIntIdGenerator _idGenerator;

    public CreateParkingSpace(
        IParkingSpaceRepository parkingSpaceRepository,
        IIntIdGenerator idGenerator)
    {
        _parkingSpaceRepository = parkingSpaceRepository
            ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));

        _idGenerator = idGenerator
            ?? throw new ArgumentNullException(nameof(idGenerator));
    }

    public async Task<Result<ParkingSpace>> ExecuteAsync(CreateParkingSpaceRequest? request)
    {

        var validationResult = ParkingSpaceRequestValidator.Validate(request);

        if (validationResult.IsFailure)
        {
            return Result<ParkingSpace>.Failure(validationResult.Error);
        }



        int newId = _idGenerator.NextId();

        var parkingSpaceResult = ParkingSpace.Create(newId, request.Name, request.Type, request.IsActive);

        if (parkingSpaceResult.IsFailure)
        {
            return parkingSpaceResult;
        }

        await _parkingSpaceRepository.AddAsync(parkingSpaceResult.Value!);

        return parkingSpaceResult;
    }
}