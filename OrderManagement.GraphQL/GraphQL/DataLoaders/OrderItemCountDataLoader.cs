using GreenDonut;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.GraphQL.GraphQL.DataLoaders;

public class OrderItemCountDataLoader : BatchDataLoader<Guid, int>
{
    private readonly ApplicationDbContext _dbContext;

    public OrderItemCountDataLoader(
        ApplicationDbContext dbContext,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dbContext = dbContext;
    }

    protected override async Task<IReadOnlyDictionary<Guid, int>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        var counts = await _dbContext.Set<OrderItem>()
            .Where(x => keys.Contains(x.OrderId))
            .GroupBy(x => x.OrderId)
            .Select(g => new
            {
                OrderId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.OrderId, x => x.Count, cancellationToken);

        return keys.ToDictionary(k => k, k => counts.GetValueOrDefault(k, 0));
    }
}
