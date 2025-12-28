using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

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

    public IQueryable<Order> GetOrdersQueryable()
    {
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

    public async Task<int> GetItemCountAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrderItem>()
            .Where(item => item.OrderId == orderId)
            .CountAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalAmountAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var items = await _context.Set<OrderItem>()
            .Where(item => item.OrderId == orderId)
            .ToListAsync(cancellationToken);

        return items.Sum(item => item.Quantity * item.UnitPrice);
    }
}
