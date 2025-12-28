namespace OrderManagement.Domain.Common;

/// <summary>
/// Represents the result of an operation with explicit success/failure handling
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyList<Error> Errors { get; }

    private Result(bool isSuccess, T? value, IReadOnlyList<Error>? errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? Array.Empty<Error>();
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Failure(params Error[] errors)
    {
        return new Result<T>(false, default, errors);
    }

    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        return new Result<T>(false, default, errors.ToList());
    }
}

/// <summary>
/// Represents an error with code, field, and message
/// </summary>
public record Error
{
    public string Code { get; init; }
    public string Field { get; init; }
    public string Message { get; init; }

    public Error(string code, string message)
        : this(code, string.Empty, message)
    {
    }

    public Error(string code, string field, string message)
    {
        Code = code;
        Field = field;
        Message = message;
    }
}
