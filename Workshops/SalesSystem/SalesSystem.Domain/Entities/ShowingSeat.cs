using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Enums;

namespace SalesSystem.Domain.Entities;

public sealed class ShowingSeat
{
    // Identity and core properties
    public Guid Id { get; }
    public int SeatNumber { get; }
    public SeatStatus Status { get; private set; }
    public bool IsAvailable => Status == SeatStatus.Available;
    public bool IsReserved => Status == SeatStatus.Reserved;
    public bool IsSold => Status == SeatStatus.Sold;

    private ShowingSeat(Guid id, int seatNumber)
    {
        Id = id;
        SeatNumber = seatNumber;
        Status = SeatStatus.Available;
    }

    // Public operations
    public static Result<ShowingSeat> Create(int seatNumber)
    {
        if (seatNumber <= 0)
        {
            return Result<ShowingSeat>.Failure(
                new Error("Seat.InvalidNumber", "Seat number must be greater than zero."));
        }

        var seat = new ShowingSeat(Guid.NewGuid(), seatNumber);

        return Result<ShowingSeat>.Success(seat);
    }
    public Result Reserve()
    {
        var result = ValidateReserve();
        if (result.IsFailure)
            return result;

        ApplyReserve();
        return Result.Success();
    }
    public Result Sell()
    {
        var result = ValidateSell();
        if (result.IsFailure)
            return result;

        ApplySell();
        return Result.Success();
    }
    public Result Release()
    {
        var result = ValidateRelease();
        if (result.IsFailure)
            return result;

        ApplyRelease();
        return Result.Success();
    }

    // Reserve operations
    private Result ValidateReserve()
    {
        if (Status != SeatStatus.Available)
        {
            return Result.Failure(
                new Error("Seat.NotAvailable", "Seat is not available."));
        }

        return Result.Success();
    }
    private void ApplyReserve()
    {
        Status = SeatStatus.Reserved;
    }

    // Sell operations
    private Result ValidateSell()
    {
        if (Status != SeatStatus.Reserved)
        {
            return Result.Failure(
                new Error("Seat.NotReserved", "Seat must be reserved first."));
        }

        return Result.Success();
    }
    private void ApplySell()
    {
        Status = SeatStatus.Sold;
    }

    // Release operations
    private Result ValidateRelease()
    {
        if (Status != SeatStatus.Reserved)
        {
            return Result.Failure(
                new Error("Seat.NotReserved", "Seat is not reserved."));
        }

        return Result.Success();
    }
    private void ApplyRelease()
    {
        Status = SeatStatus.Available;
    }
}