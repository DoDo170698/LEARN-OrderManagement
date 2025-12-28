using HotChocolate.Types;
using OrderManagement.Domain.Entities;

namespace OrderManagement.GraphQL.GraphQL.Types;

/// <summary>
/// GraphQL Object Type configuration for Order entity
/// </summary>
public class OrderType : ObjectType<Order>
{
    protected override void Configure(IObjectTypeDescriptor<Order> descriptor)
    {
        descriptor
            .Name("Order")
            .Description("Represents a customer order with items");

        descriptor
            .Field(o => o.Items)
            .UsePaging<OrderItemType>()
            .Description("Order items with pagination support");

        descriptor
            .Field(o => o.TotalAmount)
            .Description("Total amount of the order");
    }
}
