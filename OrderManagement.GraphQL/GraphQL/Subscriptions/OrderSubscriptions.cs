using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using OrderManagement.Application.DTOs;

namespace OrderManagement.GraphQL.GraphQL.Mutations;

public class OrderSubscriptions
{
    [Subscribe]
    [Topic(nameof(OnOrderCreated))]
    public OrderDto OnOrderCreated([EventMessage] OrderDto order)
    {
        return order;
    }

    [Subscribe]
    [Topic(nameof(OnOrderUpdated))]
    public OrderDto OnOrderUpdated([EventMessage] OrderDto order)
    {
        return order;
    }

    [Subscribe]
    [Topic(nameof(OnOrderDeleted))]
    public Guid OnOrderDeleted([EventMessage] Guid orderId)
    {
        return orderId;
    }
}
