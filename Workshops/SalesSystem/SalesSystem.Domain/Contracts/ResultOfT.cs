namespace SalesSystem.Domain.Contracts;

public sealed class Result<T> : Result
{
    private Result(T value)
        : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error)
        : base(false, error)
    {
        _value = default;
    }

    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failure result.");

    public static Result<T> Success(T value)
        => new Result<T>(value);

    public static new Result<T> Failure(Error error)
        => new Result<T>(error);
}