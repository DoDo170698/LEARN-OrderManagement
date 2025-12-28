using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Domain.Common;

/// <summary>
/// Converts Result failures to appropriate exceptions for GraphQL
/// </summary>
public static class ResultExceptionConverter
{
    /// <summary>
    /// Throws appropriate exception based on Result errors
    /// </summary>
    public static void ThrowIfFailure<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return;

        // Group errors by type
        var validationErrors = result.Errors.Where(e => e.Code == Error.Codes.ValidationError).ToList();
        var businessErrors = result.Errors.Where(e => e.Code != Error.Codes.ValidationError).ToList();

        // Validation errors → ValidationFailedException
        if (validationErrors.Any())
        {
            var fieldErrors = validationErrors
                .Where(e => !string.IsNullOrEmpty(e.Field))
                .GroupBy(e => e.Field)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Message).ToArray()
                );

            throw new ValidationFailedException(fieldErrors);
        }

        // Business rule errors → BusinessRuleException
        if (businessErrors.Any())
        {
            var firstError = businessErrors.First();
            throw new BusinessRuleException(firstError.Code, firstError.Message);
        }

        // Fallback
        throw new Exception("An unexpected error occurred");
    }
}
