using OrderManagement.Blazor.Resources;
using StrawberryShake;

namespace OrderManagement.Blazor.Helpers;

/// <summary>
/// Helper class to map GraphQL error codes to localized error messages
/// </summary>
public static class ErrorMessageHelper
{
    /// <summary>
    /// Gets the error message from an OperationResult
    /// </summary>
    public static string GetErrorMessage(IOperationResult result)
    {
        if (result.Errors == null || !result.Errors.Any())
        {
            return ErrorMessages.UNKNOWN_ERROR;
        }

        // Get the first error (can be enhanced to handle multiple errors)
        var error = result.Errors.First();

        // Try to get the error code from extensions
        if (error.Extensions != null && error.Extensions.TryGetValue("errorCode", out var errorCodeObj))
        {
            var errorCode = errorCodeObj?.ToString();
            if (!string.IsNullOrEmpty(errorCode))
            {
                return GetMessageByErrorCode(errorCode);
            }
        }

        // Fallback to error message
        return error.Message ?? ErrorMessages.UNKNOWN_ERROR;
    }

    /// <summary>
    /// Maps error code to localized message from resources
    /// </summary>
    public static string GetMessageByErrorCode(string errorCode)
    {
        return errorCode switch
        {
            "ORDER_NOT_FOUND" => ErrorMessages.ORDER_NOT_FOUND,
            "INVALID_INPUT" => ErrorMessages.INVALID_INPUT,
            "VALIDATION_FAILED" => ErrorMessages.VALIDATION_FAILED,
            _ => ErrorMessages.UNKNOWN_ERROR
        };
    }
}
