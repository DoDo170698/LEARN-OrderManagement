using HotChocolate.Authorization;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Queries;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;
using OrderManagement.GraphQL.GraphQL.Payloads;

namespace OrderManagement.GraphQL.GraphQL.Queries;

/// <summary>
/// GraphQL queries for Order entity
/// </summary>
public class OrderQueries
{
    /// <summary>
    /// Gets all orders with their items (with GraphQL projection support)
    /// </summary>
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Order> GetOrders([Service] IUnitOfWork unitOfWork)
    {
        return unitOfWork.Orders.GetOrdersWithItemsQueryable();
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
        var query = new GetOrdersByStatusQuery { Status = status };
        return await mediator.Send(query, cancellationToken);
    }
}
