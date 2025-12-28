using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.GraphQL.GraphQL.Resolvers;

public class OrderResolvers
{
    public async Task<int> GetItemCountAsync(
        [Parent] Order order,
        [Service] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.Orders.GetItemCountAsync(order.Id, cancellationToken);
    }

    public async Task<decimal> GetTotalAmountAsync(
        [Parent] Order order,
        [Service] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.Orders.GetTotalAmountAsync(order.Id, cancellationToken);
    }
}
