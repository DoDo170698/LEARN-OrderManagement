using HotChocolate.Types;
using OrderManagement.Domain.Entities;

namespace OrderManagement.GraphQL.GraphQL.Types;

/// <summary>
/// GraphQL Object Type configuration for OrderItem entity
/// </summary>
public class OrderItemType : ObjectType<OrderItem>
{
    protected override void Configure(IObjectTypeDescriptor<OrderItem> descriptor)
    {
        descriptor
            .Name("OrderItem")
            .Description("Represents an item within an order");

        descriptor
            .Field("subtotal")
            .Type<DecimalType>()
            .Description("Subtotal for this line item (Quantity * UnitPrice)")
            .Resolve(context =>
            {
                var item = context.Parent<OrderItem>();
                return item.Quantity * item.UnitPrice;
            });
    }
}
