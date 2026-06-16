using CleanBookingV2.Domain.Enums;
using CleanBookingV2.Domain.Policies;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.Domain.Entities;

/// <summary>
/// Represents a guest reservation for one room and optional parking.
/// The entity owns the rules that must always be true for a booking, such as valid
/// check-in time, positive guest count, and no updates after cancellation. Placing
/// these invariants in the entity is safer than leaving them only in controllers or
/// React because every backend workflow must pass through the same rules.
/// </summary>
public sealed class Booking
{
    public Guid Id { get; private set; }
    public string GuestName { get; private set; } = string.Empty;
    public DateRange Stay { get; private set; } = null!;
    public int NumberOfGuests { get; private set; }
    public int RoomId { get; private set; }
    public int? ParkingSpaceId { get; private set; }
    public decimal TotalPrice { get; private set; }
    public TimeOnly? EstimatedArrivalTime { get; private set; }
    public BookingStatus Status { get; private set; }
    public Guid Version { get; private set; }

    private Booking()
    {
    }

    /// <summary>
    /// Creates a new active booking from already-prepared booking data.
    /// Availability and price are calculated in the application layer before this
    /// constructor is called, while the entity checks invariant rules that belong to
    /// the booking itself. This separation keeps orchestration outside the entity
    /// but prevents invalid booking state from being created.
    /// </summary>
    public Booking(
        Guid id,
        string guestName,
        DateRange stay,
        int numberOfGuests,
        int roomId,
        int? parkingSpaceId,
        decimal totalPrice,
        TimeOnly? estimatedArrivalTime)
    {
        Validate(id, guestName, stay, numberOfGuests, roomId, parkingSpaceId, totalPrice, estimatedArrivalTime);

        Id = id;
        GuestName = guestName.Trim();
        Stay = stay;
        NumberOfGuests = numberOfGuests;
        RoomId = roomId;
        ParkingSpaceId = parkingSpaceId;
        TotalPrice = totalPrice;
        EstimatedArrivalTime = estimatedArrivalTime;
        Status = BookingStatus.Active;
        Version = Guid.NewGuid();
    }

    /// <summary>
    /// Updates editable booking details and marks the booking version as changed.
    /// Cancelled bookings cannot be edited because cancellation is treated as a
    /// final historical state. The method accepts the recalculated price from the
    /// application layer so the entity remains focused on invariants rather than
    /// loading room data or calculating availability.
    /// </summary>
    public void UpdateDetails(
        string guestName,
        DateRange stay,
        int numberOfGuests,
        int roomId,
        int? parkingSpaceId,
        decimal totalPrice,
        TimeOnly? estimatedArrivalTime)
    {
        if (Status == BookingStatus.Cancelled)
            throw new ArgumentException("Cancelled bookings cannot be updated.");

        Validate(Id, guestName, stay, numberOfGuests, roomId, parkingSpaceId, totalPrice, estimatedArrivalTime);

        GuestName = guestName.Trim();
        Stay = stay;
        NumberOfGuests = numberOfGuests;
        RoomId = roomId;
        ParkingSpaceId = parkingSpaceId;
        TotalPrice = totalPrice;
        EstimatedArrivalTime = estimatedArrivalTime;
        MarkModified();
    }

    /// <summary>
    /// Soft-cancels the booking.
    /// The record is kept for history instead of being deleted, but its status is
    /// changed so availability checks no longer treat it as blocking room or parking
    /// resources.
    /// </summary>
    public void Cancel()
    {
        Status = BookingStatus.Cancelled;
        MarkModified();
    }

    /// <summary>
    /// Reports whether this booking should block availability.
    /// This small helper keeps status comparisons out of callers and makes the
    /// overlap methods read like business language.
    /// </summary>
    public bool IsActive()
    {
        return Status == BookingStatus.Active;
    }

    /// <summary>
    /// Checks whether this active booking overlaps another stay.
    /// Cancelled bookings return false so historical records do not block future
    /// bookings.
    /// </summary>
    public bool Overlaps(DateRange otherStay)
    {
        return IsActive() && Stay.Overlaps(otherStay);
    }

    /// <summary>
    /// Checks whether this booking blocks a specific room for a requested stay.
    /// The room id comparison is kept here so availability services can ask the
    /// entity a business question instead of duplicating the condition.
    /// </summary>
    public bool OverlapsRoom(int roomId, DateRange otherStay)
    {
        return RoomId == roomId && Overlaps(otherStay);
    }

    /// <summary>
    /// Checks whether this booking blocks a specific parking space for a requested stay.
    /// Nullable parking is handled by the equality comparison: bookings without a
    /// parking space never block a concrete parking id.
    /// </summary>
    public bool OverlapsParkingSpace(int parkingSpaceId, DateRange otherStay)
    {
        return ParkingSpaceId == parkingSpaceId && Overlaps(otherStay);
    }

    /// <summary>
    /// Validates the invariant rules that must hold for both new and updated bookings.
    /// This central method avoids having the constructor and update method drift
    /// apart. It intentionally validates only booking-owned rules; room existence,
    /// capacity, availability, and pricing are handled by application services.
    /// </summary>
    private static void Validate(
        Guid id,
        string guestName,
        DateRange stay,
        int numberOfGuests,
        int roomId,
        int? parkingSpaceId,
        decimal totalPrice,
        TimeOnly? estimatedArrivalTime)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Booking id cannot be empty.");

        if (string.IsNullOrWhiteSpace(guestName))
            throw new ArgumentException("Guest name is required.");

        if (numberOfGuests <= 0)
            throw new ArgumentException("Number of guests must be greater than zero.");

        if (roomId <= 0)
            throw new ArgumentException("Room id must be greater than zero.");

        if (parkingSpaceId is <= 0)
            throw new ArgumentException("Parking space id must be greater than zero when provided.");

        if (totalPrice < 0)
            throw new ArgumentException("Total price cannot be negative.");

        ValidateCheckInTime(stay.Start);
        ValidateCheckOutTime(stay.End);
        ValidateLateArrivalEstimate(stay.Start, estimatedArrivalTime);
    }

    /// <summary>
    /// Validates that check-in is inside the policy window.
    /// The policy is read from BookingPolicy so the same source can also be exposed
    /// to the frontend for working-state controls.
    /// </summary>
    private static void ValidateCheckInTime(DateTime checkIn)
    {
        var time = TimeOnly.FromDateTime(checkIn);
        BookingPolicy policy = BookingPolicy.Current;

        if (time < policy.EarliestCheckIn || time > policy.LatestCheckIn)
            throw new ArgumentException($"Check-in must be between {policy.EarliestCheckIn:HH:mm} and {policy.LatestCheckIn:HH:mm}.");
    }

    /// <summary>
    /// Validates that checkout does not exceed the policy limit.
    /// Checkout is allowed earlier than the limit, but not later, because the room
    /// needs to become available for cleaning and later arrivals.
    /// </summary>
    private static void ValidateCheckOutTime(DateTime checkOut)
    {
        BookingPolicy policy = BookingPolicy.Current;
        var time = TimeOnly.FromDateTime(checkOut);

        if (time > policy.LatestCheckOut)
            throw new ArgumentException($"Check-out must be no later than {policy.LatestCheckOut:HH:mm}.");
    }

    /// <summary>
    /// Requires an estimated arrival time for late check-ins.
    /// This rule models operational needs: if a guest arrives after the normal
    /// evening threshold, staff need a more precise estimate.
    /// </summary>
    private static void ValidateLateArrivalEstimate(DateTime checkIn, TimeOnly? estimatedArrivalTime)
    {
        var time = TimeOnly.FromDateTime(checkIn);
        BookingPolicy policy = BookingPolicy.Current;

        if (time > policy.LateArrivalThreshold && estimatedArrivalTime is null)
            throw new ArgumentException($"Late arrivals after {policy.LateArrivalThreshold:HH:mm} must provide an estimated arrival time.");
    }

    /// <summary>
    /// Changes the optimistic concurrency token whenever mutable booking state changes.
    /// A GUID token is simple for SQLite and EF Core. More advanced systems might use
    /// database rowversion columns, but SQLite does not provide that same built-in
    /// pattern.
    /// </summary>
    private void MarkModified()
    {
        Version = Guid.NewGuid();
    }
}
