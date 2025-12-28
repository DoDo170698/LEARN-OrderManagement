using FluentValidation.Results;
using OrderManagement.Domain.Common;

namespace OrderManagement.Application.Common.Extensions;

/// <summary>
/// Extensions for converting FluentValidation results to Result pattern
/// </summary>
public static class ValidationResultExtensions
{
    /// <summary>
    /// Converts FluentValidation ValidationResult to Result with errors
    /// </summary>
    public static Result<T> ToFailureResult<T>(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors.Select(e =>
            new Error(ErrorCodes.ValidationError, e.PropertyName, e.ErrorMessage)
        ).ToArray();

        return Result<T>.Failure(errors);
    }
}
