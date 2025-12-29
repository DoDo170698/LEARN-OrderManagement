using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace OrderManagement.GraphQL.GraphQL.Filters;

/// <summary>
/// Converts exceptions to GraphQL standard error format
/// </summary>
public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        return error.Exception switch
        {
            DbUpdateException dbEx => HandleDatabaseError(error, dbEx),
            UnauthorizedAccessException unauthorizedEx => HandleUnauthorizedError(error, unauthorizedEx),
            _ => error
        };
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
