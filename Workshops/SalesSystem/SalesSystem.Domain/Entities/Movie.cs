using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Enums;

namespace SalesSystem.Domain.Entities;

public sealed class Movie
{
    // Identity and core properties
    public Guid Id { get; }
    public int MovieNumber { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int YearReleased  { get; private set; }
    public AgeRating AgeRating { get; private set; }
    public TimeSpan Duration { get; private set; }
    public bool IsActive { get; private set; }

    private Movie(Guid id, int movieNumber, string title, string description, int yearReleased, AgeRating ageRating, TimeSpan duration, bool isActive)
    {
        Id = id;
        MovieNumber = movieNumber;
        Title = title;
        Description = description;
        YearReleased = yearReleased;
        AgeRating = ageRating;
        Duration = duration;
        IsActive = isActive;
    }

    // Public operations
    public static Result<Movie> Create(int movieNumber, string title, string description, int yearReleased, AgeRating ageRating, TimeSpan duration, bool isActive = true)
    {
        Result result;

        result = ValidateMovieNumber(movieNumber);
        if (result.IsFailure)
            return Result<Movie>.Failure(result.Error);

        result = ValidateTitle(title);
        if (result.IsFailure)
            return Result<Movie>.Failure(result.Error);

        result = ValidateDescription(description);
        if (result.IsFailure)
            return Result<Movie>.Failure(result.Error);

        result = ValidateYearReleased(yearReleased);
        if (result.IsFailure)
            return Result<Movie>.Failure(result.Error);

        result = ValidateAgeRating(ageRating);
        if (result.IsFailure)
            return Result<Movie>.Failure(result.Error);

        result = ValidateDuration(duration);
        if (result.IsFailure)
            return Result<Movie>.Failure(result.Error);

        var movie = new Movie(
            Guid.NewGuid(),
            movieNumber,
            title,
            description,
            yearReleased,
            ageRating,
            duration,
            isActive);

        return Result<Movie>.Success(movie);
    }

    public Result Update(int movieNumber, string title, string description, int yearReleased, AgeRating ageRating, TimeSpan duration)
    {
        Result result;

        result = ValidateMovieNumberChange(movieNumber);
        if (result.IsFailure)
            return result;

        result = ValidateTitleChange(title);
        if (result.IsFailure)
            return result;

        result = ValidateDescriptionChange(description);
        if (result.IsFailure)
            return result;

        result = ValidateYearReleasedChange(yearReleased);
        if (result.IsFailure)
            return Result<Movie>.Failure(result.Error);

        result = ValidateAgeRatingChange(ageRating);
        if (result.IsFailure)
            return result;

        result = ValidateDurationChange(duration);
        if (result.IsFailure)
            return result;

        if (movieNumber != MovieNumber)
            ApplyMovieNumberChange(movieNumber);

        if (title != Title)
            ApplyTitleChange(title);

        if (description != Description)
            ApplyDescriptionChange(description);

        if (ageRating != AgeRating)
            ApplyAgeRatingChange(ageRating);

        if (duration != Duration)
            ApplyDurationChange(duration);

        return Result.Success();
    }

    public Result Activate()
    {
        var result = ValidateActivate();
        if (result.IsFailure)
            return result;

        ApplyActivate();
        return Result.Success();
    }

    public Result Deactivate()
    {
        var result = ValidateDeactivate();
        if (result.IsFailure)
            return result;

        ApplyDeactivate();
        return Result.Success();
    }

    // Movie number operations
    private Result ValidateMovieNumberChange(int movieNumber)
    {
        if (movieNumber == MovieNumber)
            return Result.Success();

        return ValidateMovieNumber(movieNumber);
    }

    private static Result ValidateMovieNumber(int movieNumber)
    {
        if (movieNumber <= 0)
        {
            return Result.Failure(
                new Error("Movie.InvalidMovieNumber", "Movie number must be greater than zero."));
        }

        return Result.Success();
    }

    private void ApplyMovieNumberChange(int movieNumber)
    {
        MovieNumber = movieNumber;
    }

    // Title operations
    private Result ValidateTitleChange(string title)
    {
        if (title == Title)
            return Result.Success();

        return ValidateTitle(title);
    }

    private static Result ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(
                new Error("Movie.InvalidTitle", "Movie title is required."));
        }

        return Result.Success();
    }

    private void ApplyTitleChange(string title)
    {
        Title = title;
    }

    // Description operations
    private Result ValidateDescriptionChange(string description)
    {
        if (description == Description)
            return Result.Success();

        return ValidateDescription(description);
    }

    private static Result ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(
                new Error("Movie.InvalidDescription", "Movie description is required."));
        }

        return Result.Success();
    }

    private void ApplyDescriptionChange(string description)
    {
        Description = description;
    }

    // Movie number operations
    private Result ValidateYearReleasedChange(int yearReleased)
    {
        if (yearReleased == YearReleased)
            return Result.Success();

        return ValidateMovieNumber(yearReleased);
    }

    private static Result ValidateYearReleased(int yearReleased)
    {
        if (yearReleased <= 0)
        {
            return Result.Failure(
                new Error("Movie.InvalidYearReleased", "YearReleased number must be greater than zero."));
        }

        return Result.Success();
    }

    private void ApplyYearReleasedChange(int yearReleased)
    {
        YearReleased = yearReleased;
    }



    // Age rating operations
    private Result ValidateAgeRatingChange(AgeRating ageRating)
    {
        if (ageRating == AgeRating)
            return Result.Success();

        return ValidateAgeRating(ageRating);
    }

    private static Result ValidateAgeRating(AgeRating ageRating)
    {
        if (!Enum.IsDefined(typeof(AgeRating), ageRating))
        {
            return Result.Failure(
                new Error("Movie.InvalidAgeRating", "Invalid age rating."));
        }

        return Result.Success();
    }

    private void ApplyAgeRatingChange(AgeRating ageRating)
    {
        AgeRating = ageRating;
    }


    // Duration operations
    private Result ValidateDurationChange(TimeSpan duration)
    {
        if (duration == Duration)
            return Result.Success();

        return ValidateDuration(duration);
    }

    private static Result ValidateDuration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return Result.Failure(
                new Error("Movie.InvalidDuration", "Movie duration must be greater than zero."));
        }

        return Result.Success();
    }

    private void ApplyDurationChange(TimeSpan duration)
    {
        Duration = duration;
    }

    // Activation operations
    private Result ValidateActivate()
    {
        if (IsActive)
        {
            return Result.Failure(
                new Error("Movie.AlreadyActive", "Movie is already active."));
        }

        return Result.Success();
    }

    private void ApplyActivate()
    {
        IsActive = true;
    }

    // Deactivation operations
    private Result ValidateDeactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(
                new Error("Movie.AlreadyInactive", "Movie is already inactive."));
        }

        return Result.Success();
    }

    private void ApplyDeactivate()
    {
        IsActive = false;
    }
}