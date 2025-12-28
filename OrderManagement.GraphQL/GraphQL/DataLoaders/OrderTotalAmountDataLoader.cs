using GreenDonut;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.GraphQL.GraphQL.DataLoaders;

public class OrderTotalAmountDataLoader : BatchDataLoader<Guid, decimal>
{
    private readonly ApplicationDbContext _dbContext;

    public OrderTotalAmountDataLoader(
        ApplicationDbContext dbContext,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContext = dbContext;
    }

    protected override async Task<IReadOnlyDictionary<Guid, decimal>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        var amounts = await _dbContext.Set<OrderItem>()
            .Where(x => keys.Contains(x.OrderId))
            .GroupBy(x => x.OrderId)
            .Select(g => new
            {
                OrderId = g.Key,
                Total = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .ToDictionaryAsync(x => x.OrderId, x => x.Total, cancellationToken);

        return keys.ToDictionary(k => k, k => amounts.GetValueOrDefault(k, 0m));
    }
}
