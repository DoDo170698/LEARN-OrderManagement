using HotChocolate;
using OrderManagement.GraphQL.GraphQL.Payloads;

namespace OrderManagement.GraphQL.GraphQL.Filters;

/// <summary>
/// Error filter to add error codes to GraphQL errors
/// </summary>
public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        // Check if the exception is OrderNotFoundError
        if (error.Exception is OrderNotFoundError orderNotFoundError)
        {
            return error.WithExtensions(new Dictionary<string, object?>
            {
                { "errorCode", orderNotFoundError.ErrorCode },
                { "orderId", orderNotFoundError.OrderId }
            });
        }

        // For other exceptions, return as is
        return error;
    }
}
