namespace SalesSystem.Domain.Contracts;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Success cannot have error");

        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Failure must have error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success()
        => new Result(true, Error.None);

    public static Result Failure(Error error)
        => new Result(false, error);

    public Result<T> ToFailure<T>()
    {
        return Result<T>.Failure(Error);
    }
}
