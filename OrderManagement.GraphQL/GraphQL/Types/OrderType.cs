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

        // Configure items field with pagination support
        descriptor
            .Field(o => o.Items)
            .UsePaging<OrderItemType>()
            .Description("Order items with pagination support");

        // Add computed field: totalAmount
        descriptor
            .Field("totalAmount")
            .Type<DecimalType>()
            .Description("Total amount calculated from all order items")
            .Resolve(context =>
            {
                var order = context.Parent<Order>();
                return order.Items?.Sum(item => item.Quantity * item.UnitPrice) ?? 0;
            });
    }
}
