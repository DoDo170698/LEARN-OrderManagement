using HotChocolate;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.GraphQL.GraphQL.Filters;

/// <summary>
/// Converts exceptions to GraphQL standard error format with top-level errors
/// Only handles business exceptions. Auth/validation errors from HotChocolate are preserved.
/// </summary>
public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        return error.Exception switch
        {
            ValidationFailedException validationEx => HandleValidationError(error, validationEx),
            BusinessRuleException businessEx => HandleBusinessRuleError(error, businessEx),
            DbUpdateException dbEx => HandleDatabaseError(error, dbEx),
            UnauthorizedAccessException unauthorizedEx => HandleUnauthorizedError(error, unauthorizedEx),
            _ => error
        };
    }

    private static IError HandleValidationError(IError error, ValidationFailedException exception)
    {
        return error.WithMessage("Validation failed")
            .WithExtensions(new Dictionary<string, object?>
            {
                { "code", Domain.Common.ErrorCodes.ValidationError },
                { "fields", exception.FieldErrors }
            });
    }

    private static IError HandleBusinessRuleError(IError error, BusinessRuleException exception)
    {
        return error.WithMessage(exception.Message)
            .WithExtensions(new Dictionary<string, object?>
            {
                { "code", exception.ErrorCode }
            });
    }

    private static IError HandleDatabaseError(IError error, DbUpdateException exception)
    {
        return error.WithMessage("A database error occurred")
            .WithExtensions(new Dictionary<string, object?>
            {
                { "code", Domain.Common.ErrorCodes.DatabaseError }
            });
    }

    private static IError HandleUnauthorizedError(IError error, UnauthorizedAccessException exception)
    {
        return error.WithMessage("Unauthorized")
            .WithExtensions(new Dictionary<string, object?>
            {
                { "code", Domain.Common.ErrorCodes.Unauthorized }
            });
    }
}
