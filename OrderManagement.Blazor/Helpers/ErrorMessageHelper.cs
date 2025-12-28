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

        var error = result.Errors.First();

        if (error.Extensions != null && error.Extensions.TryGetValue("errorCode", out var errorCodeObj))
        {
            var errorCode = errorCodeObj?.ToString();
            if (!string.IsNullOrEmpty(errorCode))
            {
                return GetMessageByErrorCode(errorCode);
            }
        }

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
