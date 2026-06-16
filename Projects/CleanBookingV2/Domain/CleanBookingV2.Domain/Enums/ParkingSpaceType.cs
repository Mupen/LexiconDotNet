namespace CleanBookingV2.Domain.Enums;

/// <summary>
/// Classifies parking spaces for display and future feature growth.
/// It is currently simple, but using an enum keeps the API contract explicit and
/// leaves room for later categories such as EV charging or accessible parking.
/// </summary>
public enum ParkingSpaceType
{
    Standard = 1
}
