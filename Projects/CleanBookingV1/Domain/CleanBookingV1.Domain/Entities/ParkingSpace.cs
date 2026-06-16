using CleanBookingV1.Domain.Contracts;
using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Domain.Entities;

public sealed class ParkingSpace
{
    public int Id { get; }
    public string Name { get; private set; }
    public ParkingSpaceType Type { get; private set; }
    public bool IsActive { get; private set; }

    private ParkingSpace(int id, string name, ParkingSpaceType type, bool isActive)
    {
        Id = id;
        Name = name;
        Type = type;
        IsActive = isActive;
    }

    public static Result<ParkingSpace> Create(int id, string name, ParkingSpaceType type, bool isActive = true)
    {
        if (id <= 0)
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.InvalidId", "Id must be greater than zero."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.InvalidName", "Name is required."));
        }

        if (!Enum.IsDefined(typeof(ParkingSpaceType), type))
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.InvalidType", "Invalid parking space type."));
        }

        var parkingSpace = new ParkingSpace(id, name, type, isActive);

        return Result<ParkingSpace>.Success(parkingSpace);
    }

    public Result UpdateDetails(string name, ParkingSpaceType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.InvalidName", "Name is required."));
        }

        if (Enum.IsDefined(typeof(ParkingSpaceType), type))
        {
            return Result<ParkingSpace>.Failure(
                new Error("ParkingSpace.InvalidType", "Parking space type is required."));
        }

        Name = name;
        Type = type;

        return Result.Success();
    }


    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}