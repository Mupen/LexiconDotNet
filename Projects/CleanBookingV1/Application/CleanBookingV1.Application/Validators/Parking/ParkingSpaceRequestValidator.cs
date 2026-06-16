using CleanBookingV1.Application.Requests;
using CleanBookingV1.Application.Requests.Parking;
using CleanBookingV1.Domain.Contracts;

public static class ParkingSpaceRequestValidator
{
    public static Result Validate(CreateParkingSpaceRequest? request)
    {
        if (request is null)
            return Result.Failure(new Error("ParkingSpace.Create.NullRequest", "Request cannot be null."));

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure(new Error("ParkingSpace.Create.InvalidName", "Name is required."));

        return Result.Success();
    }

    public static Result Validate(UpdateParkingSpaceRequest? request)
    {
        if (request is null)
            return Result.Failure(new Error("ParkingSpace.Update.NullRequest", "Request cannot be null."));

        if (request.ParkingSpaceId <= 0)
            return Result.Failure(new Error("ParkingSpace.Update.InvalidId", "Parking space id must be greater than zero."));

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure(new Error("ParkingSpace.Update.InvalidName", "Name is required."));

        return Result.Success();
    }

    public static Result Validate(DeleteParkingSpaceRequest? request)
    {
        if (request is null)
            return Result.Failure(new Error("ParkingSpace.Delete.NullRequest", "Request cannot be null."));

        if (request.ParkingSpaceId <= 0)
            return Result.Failure(new Error("ParkingSpace.Delete.InvalidId", "Parking space id must be greater than zero."));

        return Result.Success();
    }
}