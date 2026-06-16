namespace CleanBookingV1.Domain.ValueObjects;

public sealed class DateRange
{
    public DateRange(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            throw new ArgumentException("End must be later than start.");
        }

        Start = start;
        End = end;
    }

    public DateTime Start { get; }
    public DateTime End { get; }

    public bool Overlaps(DateRange other)
    {
        return Start < other.End && other.Start < End;
    }

    public int GetNumberOfNights()
    {
        return (End.Date - Start.Date).Days;
    }

}