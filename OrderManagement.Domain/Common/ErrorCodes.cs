namespace OrderManagement.Domain.Common;

/// <summary>
/// Common error codes used across the application
/// </summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string DatabaseError = "DATABASE_ERROR";
}
