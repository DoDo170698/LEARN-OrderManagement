namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Exception thrown when business rule is violated
/// Converts to GraphQL error with extensions.code
/// </summary>
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
