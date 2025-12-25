using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for OrderItem entity
/// </summary>
public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.OrderId == orderId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var items = await _dbSet
            .Where(i => i.OrderId == orderId)
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(items);
    }
}
