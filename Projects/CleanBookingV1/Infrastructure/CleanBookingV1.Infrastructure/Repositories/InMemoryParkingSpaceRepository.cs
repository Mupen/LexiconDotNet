using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;
using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Infrastructure.Repositories;

public sealed class InMemoryParkingSpaceRepository : IParkingSpaceRepository
{
    private readonly List<ParkingSpace> _parkingSpaces =
    [
        ParkingSpace.Create(1, "P1", ParkingSpaceType.Standard, true).Value!,
        ParkingSpace.Create(2, "P2", ParkingSpaceType.Disabled, true).Value!
    ];

    public Task<IReadOnlyList<ParkingSpace>> GetAllAsync()
    {
        IReadOnlyList<ParkingSpace> result = _parkingSpaces.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<ParkingSpace?> GetByIdAsync(int id)
    {
        ParkingSpace? parkingSpace = _parkingSpaces.FirstOrDefault(space => space.Id == id);
        return Task.FromResult(parkingSpace);
    }

    public Task AddAsync(ParkingSpace parkingSpace)
    {
        ArgumentNullException.ThrowIfNull(parkingSpace);

        _parkingSpaces.Add(parkingSpace);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(ParkingSpace parkingSpace)
    {
        ArgumentNullException.ThrowIfNull(parkingSpace);

        int index = _parkingSpaces.FindIndex(existing => existing.Id == parkingSpace.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"ParkingSpace with id '{parkingSpace.Id}' was not found.");
        }

        _parkingSpaces[index] = parkingSpace;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        ParkingSpace? parkingSpace = _parkingSpaces.FirstOrDefault(space => space.Id == id);

        if (parkingSpace is null)
        {
            throw new InvalidOperationException(
                $"ParkingSpace with id '{id}' was not found.");
        }

        _parkingSpaces.Remove(parkingSpace);

        return Task.CompletedTask;
    }
}