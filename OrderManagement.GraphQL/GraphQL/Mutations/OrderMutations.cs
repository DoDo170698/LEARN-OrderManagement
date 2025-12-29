using AutoMapper;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Commands;
using OrderManagement.Domain.Common;
using OrderManagement.GraphQL.GraphQL.Inputs;

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
    public async Task<OrderDto> CreateOrderAsync(
        CreateOrderInput input,
        [Service] IMapper mapper,
        [Service] IMediator mediator,
        [Service] ITopicEventSender eventSender,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CreateOrderCommand>(input);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var firstError = result.Errors.First();
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(firstError.Message)
                    .SetExtension("code", firstError.Code)
                    .Build());
        }

        await eventSender.SendAsync(nameof(OrderSubscriptions.OnOrderCreated), result.Value!, cancellationToken);
        return result.Value!;
    }

    /// <summary>
    /// Updates an existing order
    /// </summary>
    [Authorize]
    public async Task<OrderDto> UpdateOrderAsync(
        UpdateOrderInput input,
        [Service] IMapper mapper,
        [Service] IMediator mediator,
        [Service] ITopicEventSender eventSender,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<UpdateOrderCommand>(input);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var firstError = result.Errors.First();
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(firstError.Message)
                    .SetExtension("code", firstError.Code)
                    .Build());
        }

        await eventSender.SendAsync(nameof(OrderSubscriptions.OnOrderUpdated), result.Value!, cancellationToken);
        return result.Value!;
    }

    /// <summary>
    /// Deletes an order and all its items
    /// </summary>
    [Authorize]
    public async Task<bool> DeleteOrderAsync(
        Guid id,
        [Service] IMediator mediator,
        [Service] ITopicEventSender eventSender,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteOrderCommand { Id = id }, cancellationToken);

        if (!result.IsSuccess)
        {
            var firstError = result.Errors.First();
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(firstError.Message)
                    .SetExtension("code", firstError.Code)
                    .Build());
        }

        await eventSender.SendAsync(nameof(OrderSubscriptions.OnOrderDeleted), id, cancellationToken);
        return result.Value;
    }
}
