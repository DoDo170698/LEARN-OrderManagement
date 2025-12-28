using OrderManagement.Domain.Entities;
using OrderManagement.GraphQL.GraphQL.DataLoaders;

namespace OrderManagement.GraphQL.GraphQL.Resolvers;

public class OrderResolvers
{
    public async Task<int> GetItemCountAsync(
        [Parent] Order order,
        OrderItemCountDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(order.Id, cancellationToken);
    }

    public async Task<decimal> GetTotalAmountAsync(
        [Parent] Order order,
        OrderTotalAmountDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(order.Id, cancellationToken);
    }
}
