namespace OrderManagement.Domain.Common;

/// <summary>
/// Extension methods for Result pattern
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Creates a not found result
    /// </summary>
    public static Result<T> NotFound<T>(string entityName, Guid id)
    {
        return Result<T>.Failure(
            new Error(ErrorCodes.NotFound, $"{entityName} with ID '{id}' was not found.")
        );
    }

    /// <summary>
    /// Creates a validation error result
    /// </summary>
    public static Result<T> ValidationError<T>(string field, string message)
    {
        return Result<T>.Failure(
            new Error(ErrorCodes.ValidationError, field, message)
        );
    }

    /// <summary>
    /// Gets the value from Result or throws appropriate exception
    /// Converts Result failures to GraphQL standard exceptions
    /// </summary>
    public static T GetValueOrThrow<T>(this Result<T> result)
    {
        ResultExceptionConverter.ThrowIfFailure(result);
        return result.Value!;
    }
}
