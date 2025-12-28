using OrderManagement.Blazor.Resources;
using StrawberryShake;

namespace OrderManagement.Blazor.Helpers;

public static class ErrorMessageHelper
{
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

    public static string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException =>
                "Network error. Please check your connection and try again.",

            TaskCanceledException or OperationCanceledException =>
                "Request timed out. Please try again.",

            UnauthorizedAccessException =>
                "Unauthorized. Please check your authentication token.",

            InvalidOperationException invalidOpEx =>
                $"Invalid operation: {invalidOpEx.Message}",

            _ => $"Unexpected error: {exception.Message}"
        };
    }

    public static string HandleException(Exception exception, string context = "")
    {
        Console.WriteLine($"[ERROR] {context}: {exception.GetType().Name} - {exception.Message}");
        return GetErrorMessage(exception);
    }
}
