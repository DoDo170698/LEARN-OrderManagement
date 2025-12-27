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
        return _dbSet
            .OrderByDescending(o => o.CreatedAt);
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Where(o => o.CustomerEmail == email)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
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
