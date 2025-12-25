using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using OrderManagement.Application.DTOs;

namespace OrderManagement.GraphQL.GraphQL.Mutations;

/// <summary>
/// GraphQL subscriptions for realtime order updates
/// </summary>
public class OrderSubscriptions
{
    /// <summary>
    /// Subscribe to order creation events
    /// </summary>
    [Subscribe]
    [Topic(nameof(OnOrderCreated))]
    public OrderDto OnOrderCreated([EventMessage] OrderDto order)
    {
        return order;
    }

    /// <summary>
    /// Subscribe to order update events
    /// </summary>
    [Subscribe]
    [Topic(nameof(OnOrderUpdated))]
    public OrderDto OnOrderUpdated([EventMessage] OrderDto order)
    {
        return order;
    }
}
