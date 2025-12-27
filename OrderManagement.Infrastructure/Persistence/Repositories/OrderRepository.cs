using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order entity
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<Order> GetOrdersWithItemsQueryable()
    {
        return _dbSet
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt);
    }

    public IQueryable<Order> GetOrdersQueryable()
    {
        // NO Include - optimized for list view without items
        // NO default OrderBy - GraphQL [UseSorting] middleware will handle sorting
        return _dbSet;
    }

    public async Task<int> GetOrderCountByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year + 1, 1, 1);

        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
            .CountAsync(cancellationToken);
    }
}
