namespace Viora.Domain.Abstractions;

public class Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; init; } = null!;

    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.NoError)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.NoError)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    // static members to represent success and failure objects
    public static Result<T> Success<T>(T value) => new(value, true, Error.NoError);
    public static Result Success() => new(true, Error.NoError);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<T>(T value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    private readonly T _value = value;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value of a failure result.");
}