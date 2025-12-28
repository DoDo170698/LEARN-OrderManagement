using HotChocolate.Types;
using OrderManagement.Domain.Entities;
using OrderManagement.GraphQL.GraphQL.Resolvers;

namespace OrderManagement.GraphQL.GraphQL.Types;

public class OrderType : ObjectType<Order>
{
    protected override void Configure(IObjectTypeDescriptor<Order> descriptor)
    {
        descriptor
            .Name("Order")
            .Description("Represents a customer order with items");

        descriptor
            .Field("totalAmount")
            .Type<DecimalType>()
            .ResolveWith<OrderResolvers>(r => r.GetTotalAmountAsync(default!, default!, default!));

        descriptor
            .Field("itemCount")
            .Type<IntType>()
            .ResolveWith<OrderResolvers>(r => r.GetItemCountAsync(default!, default!, default!));
    }
}
