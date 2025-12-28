namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails with field-level errors
/// Converts to GraphQL error with extensions.fields
/// </summary>
public class ValidationFailedException : Exception
{
    public Dictionary<string, string[]> FieldErrors { get; }

    public ValidationFailedException(Dictionary<string, string[]> fieldErrors)
        : base("Validation failed")
    {
        FieldErrors = fieldErrors;
    }
}
