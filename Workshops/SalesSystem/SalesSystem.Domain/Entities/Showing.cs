using SalesSystem.Domain.Contracts;

namespace SalesSystem.Domain.Entities;

public sealed class Showing
{
    // Identity and core properties
    public Guid Id { get; }
    public Guid MovieId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public IReadOnlyList<ShowingSeat> Seats => _seats.AsReadOnly();
    public bool IsCancelled { get; private set; }

    private readonly List<ShowingSeat> _seats = [];

    private Showing(Guid id, Guid movieId, DateOnly date, TimeOnly startTime, List<ShowingSeat> seats)
    {
        Id = id;
        MovieId = movieId;
        Date = date;
        StartTime = startTime;
        _seats = seats;
        IsCancelled = false;
    }

    // Public operations
    public static Result<Showing> Create(Guid movieId, DateOnly date, TimeOnly startTime, int seatCount)
    {
        Result result;

        result = ValidateMovieId(movieId);
        if (result.IsFailure)
            return Result<Showing>.Failure(result.Error);

        result = ValidateDate(date);
        if (result.IsFailure)
            return Result<Showing>.Failure(result.Error);

        result = ValidateStartTime(startTime);
        if (result.IsFailure)
            return Result<Showing>.Failure(result.Error);

        result = ValidateSeatCount(seatCount);
        if (result.IsFailure)
            return Result<Showing>.Failure(result.Error);

        var seatsResult = CreateSeats(seatCount);
        if (seatsResult.IsFailure)
            return Result<Showing>.Failure(seatsResult.Error);

        var showing = new Showing(
            Guid.NewGuid(),
            movieId,
            date,
            startTime,
            seatsResult.Value);

        return Result<Showing>.Success(showing);
    }

    public Result Update(Guid movieId, DateOnly date, TimeOnly startTime)
    {
        Result result;

        result = ValidateMovieIdChange(movieId);
        if (result.IsFailure)
            return result;

        result = ValidateDateChange(date);
        if (result.IsFailure)
            return result;

        result = ValidateStartTimeChange(startTime);
        if (result.IsFailure)
            return result;

        if (movieId != MovieId)
            ApplyMovieIdChange(movieId);

        if (date != Date)
            ApplyDateChange(date);

        if (startTime != StartTime)
            ApplyStartTimeChange(startTime);

        return Result.Success();
    }

    public Result Cancel()
    {
        var result = ValidateCancel();
        if (result.IsFailure)
            return result;

        ApplyCancel();
        return Result.Success();
    }

    public Result Restore()
    {
        var result = ValidateRestore();
        if (result.IsFailure)
            return result;

        ApplyRestore();
        return Result.Success();
    }

    // MovieId operations
    private Result ValidateMovieIdChange(Guid movieId)
    {
        if (movieId == MovieId)
            return Result.Success();

        return ValidateMovieId(movieId);
    }

    private static Result ValidateMovieId(Guid movieId)
    {
        if (movieId == Guid.Empty)
        {
            return Result.Failure(
                new Error("Showing.InvalidMovieId", "Movie id is required."));
        }

        return Result.Success();
    }

    private void ApplyMovieIdChange(Guid movieId)
    {
        MovieId = movieId;
    }

    // Date operations
    private Result ValidateDateChange(DateOnly date)
    {
        if (date == Date)
            return Result.Success();

        return ValidateDate(date);
    }

    private static Result ValidateDate(DateOnly date)
    {
        if (date == default)
        {
            return Result.Failure(
                new Error("Showing.InvalidDate", "Date is required."));
        }

        return Result.Success();
    }

    private void ApplyDateChange(DateOnly date)
    {
        Date = date;
    }

    // Start time operations
    private Result ValidateStartTimeChange(TimeOnly startTime)
    {
        if (startTime == StartTime)
            return Result.Success();

        return ValidateStartTime(startTime);
    }

    private static Result ValidateStartTime(TimeOnly startTime)
    {
        if (startTime == default)
        {
            return Result.Failure(
                new Error("Showing.InvalidStartTime", "Start time is required."));
        }

        return Result.Success();
    }

    private void ApplyStartTimeChange(TimeOnly startTime)
    {
        StartTime = startTime;
    }

    // Seat operations
    private static Result ValidateSeatCount(int seatCount)
    {
        if (seatCount <= 0 || seatCount > 54)
        {
            return Result.Failure(
                new Error("Showing.InvalidSeatCount", "Seat count must be between 1 and 54."));
        }

        return Result.Success();
    }

    private static Result<List<ShowingSeat>> CreateSeats(int seatCount)
    {
        var seats = new List<ShowingSeat>();

        for (int i = 1; i <= seatCount; i++)
        {
            var seatResult = ShowingSeat.Create(i);

            if (seatResult.IsFailure)
                return Result<List<ShowingSeat>>.Failure(seatResult.Error);

            seats.Add(seatResult.Value);
        }

        return Result<List<ShowingSeat>>.Success(seats);
    }

    // Cancellation operations
    private Result ValidateCancel()
    {
        if (IsCancelled)
        {
            return Result.Failure(
                new Error("Showing.AlreadyCancelled", "Showing is already cancelled."));
        }

        return Result.Success();
    }

    private void ApplyCancel()
    {
        IsCancelled = true;
    }

    // Restore operations
    private Result ValidateRestore()
    {
        if (!IsCancelled)
        {
            return Result.Failure(
                new Error("Showing.NotCancelled", "Showing is not cancelled."));
        }

        return Result.Success();
    }

    private void ApplyRestore()
    {
        IsCancelled = false;
    }
}