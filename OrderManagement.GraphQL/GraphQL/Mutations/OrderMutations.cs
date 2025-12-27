using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Commands;
using OrderManagement.GraphQL.GraphQL.Inputs;
using OrderManagement.GraphQL.GraphQL.Payloads;

namespace OrderManagement.GraphQL.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for Order entity
/// </summary>
[ExtendObjectType("Mutation")]
public class OrderMutations
{
    /// <summary>
    /// Creates a new order with items
    /// </summary>
    [Authorize]
    public async Task<CreateOrderPayload> CreateOrderAsync(
        CreateOrderInput input,
        [Service] IMediator mediator,
        [Service] ITopicEventSender eventSender,
        CancellationToken cancellationToken)
    {
        // Convert input to command
        var command = new CreateOrderCommand
        {
            CustomerName = input.CustomerName,
            CustomerEmail = input.CustomerEmail,
            Items = input.Items.Select(i => new CreateOrderItemDto
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        var orderDto = await mediator.Send(command, cancellationToken);

        // Send realtime event (convert DTO back to entity-like structure for subscription)
        await eventSender.SendAsync(nameof(OrderSubscriptions.OnOrderCreated), orderDto, cancellationToken);

        return new CreateOrderPayload(orderDto);
    }

    /// <summary>
    /// Updates an existing order
    /// </summary>
    [Authorize]
    public async Task<UpdateOrderPayload> UpdateOrderAsync(
        UpdateOrderInput input,
        [Service] IMediator mediator,
        [Service] ITopicEventSender eventSender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderCommand
        {
            Id = input.Id,
            CustomerName = input.CustomerName,
            CustomerEmail = input.CustomerEmail,
            Status = (OrderManagement.Domain.Enums.OrderStatus?)input.Status,
            Items = input.Items?.Select(i => new CreateOrderItemDto
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        var orderDto = await mediator.Send(command, cancellationToken);

        // Send realtime event
        await eventSender.SendAsync(nameof(OrderSubscriptions.OnOrderUpdated), orderDto, cancellationToken);

        return new UpdateOrderPayload(orderDto);
    }

    /// <summary>
    /// Deletes an order and all its items
    /// </summary>
    [Authorize]
    public async Task<bool> DeleteOrderAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteOrderCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
        {
            throw new OrderNotFoundError(id);
        }

        return result;
    }
}
