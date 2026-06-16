using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.Interfaces;

public interface IParkingSpaceRepository
{
    Task<IReadOnlyList<ParkingSpace>> GetAllAsync();
    Task<ParkingSpace?> GetByIdAsync(int id);
    Task AddAsync(ParkingSpace parkingSpace);
    Task UpdateAsync(ParkingSpace parkingSpace);
    Task DeleteAsync(int id);
}