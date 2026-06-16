using CleanBookingV1.Domain.ValueObjects;

namespace CleanBookingV1.Domain.Entities;

public sealed class Booking
{
    public Guid Id { get; }
    public string GuestName { get; private set; }
    public DateRange Stay { get; private set; }
    public int NumberOfGuests { get; private set; }

    public int? RoomId { get; private set; }
    public int? ParkingSpaceId { get; private set; }

    public decimal TotalPrice { get; private set; }

    public TimeOnly? EstimatedArrivalTime { get; private set; }

    public Booking(
        Guid id,
        string guestName,
        DateRange stay,
        int numberOfGuests,
        int? roomId,
        int? parkingSpaceId,
        decimal totalPrice,
        TimeOnly? estimatedArrivalTime)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Booking id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(guestName))
        {
            throw new ArgumentException("Guest name is required.");
        }

        if (numberOfGuests <= 0)
        {
            throw new ArgumentException("Number of guests must be greater than zero.");
        }

        if (roomId is <= 0)
        {
            throw new ArgumentException("Room id must be greater than zero when provided.");
        }

        if (parkingSpaceId is <= 0)
        {
            throw new ArgumentException("Parking space id must be greater than zero when provided.");
        }

        if (roomId is null && parkingSpaceId is null)
        {
            throw new ArgumentException("A booking must include at least a room or a parking space.");
        }

        if (totalPrice < 0)
        {
            throw new ArgumentException("Total price cannot be negative.");
        }

        if (roomId is not null)
        {
            ValidateRoomStayRules(stay.Start, stay.End, estimatedArrivalTime);
        }

        Id = id;
        GuestName = guestName;
        Stay = stay;
        NumberOfGuests = numberOfGuests;
        RoomId = roomId;
        ParkingSpaceId = parkingSpaceId;
        TotalPrice = totalPrice;
        EstimatedArrivalTime = estimatedArrivalTime;
    }

    public void UpdateDetails(
        string guestName,
        DateRange stay,
        int numberOfGuests,
        int? roomId,
        int? parkingSpaceId,
        decimal totalPrice,
        TimeOnly? estimatedArrivalTime)
    {
        if (string.IsNullOrWhiteSpace(guestName))
        {
            throw new ArgumentException("Guest name is required.");
        }

        if (numberOfGuests <= 0)
        {
            throw new ArgumentException("Number of guests must be greater than zero.");
        }

        if (roomId is <= 0)
        {
            throw new ArgumentException("Room id must be greater than zero when provided.");
        }

        if (parkingSpaceId is <= 0)
        {
            throw new ArgumentException("Parking space id must be greater than zero when provided.");
        }

        if (roomId is null && parkingSpaceId is null)
        {
            throw new ArgumentException("A booking must include at least a room or a parking space.");
        }

        if (totalPrice < 0)
        {
            throw new ArgumentException("Total price cannot be negative.");
        }

        if (roomId is not null)
        {
            ValidateRoomStayRules(stay.Start, stay.End, estimatedArrivalTime);
        }

        GuestName = guestName;
        Stay = stay;
        NumberOfGuests = numberOfGuests;
        RoomId = roomId;
        ParkingSpaceId = parkingSpaceId;
        TotalPrice = totalPrice;
        EstimatedArrivalTime = estimatedArrivalTime;
    }

    public bool HasRoom()
    {
        return RoomId.HasValue;
    }

    public bool HasParkingSpace()
    {
        return ParkingSpaceId.HasValue;
    }

    public bool UsesRoom(int roomId)
    {
        return RoomId == roomId;
    }

    public bool UsesParkingSpace(int parkingSpaceId)
    {
        return ParkingSpaceId == parkingSpaceId;
    }

    public bool Overlaps(DateRange otherStay)
    {
        return Stay.Overlaps(otherStay);
    }

    public bool OverlapsRoom(int roomId, DateRange otherStay)
    {
        return RoomId == roomId && Stay.Overlaps(otherStay);
    }

    public bool OverlapsParkingSpace(int parkingSpaceId, DateRange otherStay)
    {
        return ParkingSpaceId == parkingSpaceId && Stay.Overlaps(otherStay);
    }

    public void SetTotalPrice(decimal totalPrice)
    {
        if (totalPrice < 0)
        {
            throw new ArgumentException("Total price cannot be negative.");
        }

        TotalPrice = totalPrice;
    }

    public void CalculateTotalPrice(decimal basePricePerNight)
    {
        TotalPrice = basePricePerNight * Stay.GetNumberOfNights();
    }

    private static void ValidateRoomStayRules(
        DateTime checkIn,
        DateTime checkOut,
        TimeOnly? estimatedArrivalTime)
    {
        ValidateCheckInTime(checkIn);
        ValidateCheckOutTime(checkOut);
        ValidateLateArrivalEstimate(checkIn, estimatedArrivalTime);
    }

    private static void ValidateCheckInTime(DateTime checkIn)
    {
        var time = TimeOnly.FromDateTime(checkIn);
        var earliest = new TimeOnly(14, 0);
        var latest = new TimeOnly(22, 30);

        if (time < earliest || time > latest)
        {
            throw new ArgumentException("Check-in must be between 14:00 and 22:30.");
        }
    }

    private static void ValidateCheckOutTime(DateTime checkOut)
    {
        var latest = new TimeOnly(12, 0);
        var time = TimeOnly.FromDateTime(checkOut);

        if (time > latest)
        {
            throw new ArgumentException("Check-out must be no later than 12:00.");
        }
    }

    private static void ValidateLateArrivalEstimate(
        DateTime checkIn,
        TimeOnly? estimatedArrivalTime)
    {
        var time = TimeOnly.FromDateTime(checkIn);
        var lateArrivalThreshold = new TimeOnly(20, 0);

        if (time > lateArrivalThreshold && estimatedArrivalTime is null)
        {
            throw new ArgumentException(
                "Late arrivals after 20:00 must provide an estimated arrival time.");
        }
    }
}