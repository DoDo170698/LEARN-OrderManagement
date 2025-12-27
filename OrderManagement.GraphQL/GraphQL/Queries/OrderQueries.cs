using HotChocolate.Authorization;
using HotChocolate.Data;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Queries;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.GraphQL.GraphQL.Payloads;

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
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Order>> GetOrdersAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // ✅ CORRECT: Go through CQRS layer
        var query = new GetOrdersQuery();
        return await mediator.Send(query, cancellationToken);
    }

    /// <summary>
    /// Gets a specific order by ID
    /// </summary>
    [Authorize]
    public async Task<OrderDto> GetOrderByIdAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // ✅ CORRECT: Go through CQRS layer
        var query = new GetOrderByIdQuery { Id = id };
        var order = await mediator.Send(query, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundError(id);
        }

        return order;
    }

    /// <summary>
    /// Gets orders filtered by status
    /// </summary>
    [Authorize]
    public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(
        OrderStatus status,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // ✅ CORRECT: Go through CQRS layer
        var query = new GetOrdersByStatusQuery { Status = status };
        return await mediator.Send(query, cancellationToken);
    }

    /// <summary>
    /// Gets order items for a specific order - supports paging, filtering, and sorting
    /// </summary>
    [Authorize]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<OrderItem>> GetOrderItemsAsync(
        Guid orderId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // ✅ CORRECT: Go through CQRS layer
        var query = new GetOrderItemsQuery { OrderId = orderId };
        return await mediator.Send(query, cancellationToken);
    }
}
