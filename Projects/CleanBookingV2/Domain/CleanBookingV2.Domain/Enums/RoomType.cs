namespace CleanBookingV2.Domain.Enums;

/// <summary>
/// Classifies rooms for display and future filtering.
/// The enum keeps room category values consistent across seed data, API responses,
/// and frontend rendering instead of passing arbitrary strings through the system.
/// </summary>
public enum RoomType
{
    Single = 1,
    Double = 2,
    Family = 3
}
