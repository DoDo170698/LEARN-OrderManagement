using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Queries;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.GraphQL.GraphQL.Queries;

/// <summary>
/// GraphQL queries for Order entity
/// IMPORTANT: All queries go through CQRS layer (MediatR) for consistency
/// </summary>
public class OrderQueries
{
    /// <summary>
    /// Gets all orders WITHOUT items (optimized for list view) - supports paging, filtering, and sorting
    /// </summary>
    [Authorize]
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Order>> GetOrdersAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery();
        return await mediator.Send(query, cancellationToken);
    }

    /// <summary>
    /// Gets a specific order by ID with items
    /// </summary>
    [Authorize]
    public async Task<OrderDto> GetOrderByIdAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderByIdQuery { Id = id }, cancellationToken);

        if (!result.IsSuccess)
        {
            var firstError = result.Errors.First();
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(firstError.Message)
                    .SetCode(firstError.Code)
                    .Build());
        }

        return result.Value!;
    }
}
